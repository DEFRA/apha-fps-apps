using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Apha.Common.Constants;
using Apha.Common.Utilities.EventPublisher;
using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.Storage;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Enums;
using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Apha.FPS.Application.Services
{
    public class BulkRatesRequestService : IBulkRatesRequestService
    {
        // Status names as stored in fps.job_status.statusname
        private const string StatusInitiated = "Initiated";
        private const string StatusReleasedForApproval = "ReleasedForApproval";
        private const string StatusRejected = "Rejected";
        private const string StatusApproved = "Approved";
        private const string StatusCancelled = "Cancelled";
        private const string StatusFailed = "Failed";

        private readonly IBulkRatesRepository _repository;
        private readonly BulkRatesExcelParser _parser;
        private readonly IBulkTestRatesService _testService;
        private readonly IBulkStaffRatesService _staffService;
        private readonly IBulkAnimalRatesService _animalService;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IBulkRatesNotificationService _notificationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BulkRatesRequestService> _logger;

        public BulkRatesRequestService(
            IBulkRatesRepository repository,
            BulkRatesExcelParser parser,
            IBulkTestRatesService testService,
            IBulkStaffRatesService staffService,
            IBulkAnimalRatesService animalService,
            IEventPublisherService eventPublisherService,
            IBulkRatesNotificationService notificationService,
            IExcelExportService excelExportService,
            IS3StorageService s3StorageService,
            IConfiguration configuration,
            ILogger<BulkRatesRequestService> logger)
        {
            _repository = repository;
            _parser = parser;
            _testService = testService;
            _staffService = staffService;
            _animalService = animalService;
            _eventPublisherService = eventPublisherService;
            _notificationService = notificationService;
            _excelExportService = excelExportService;
            _s3StorageService = s3StorageService;
            _configuration = configuration;
            _logger = logger;
        }

        // ── Create request ────────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> CreateRequestAsync(
            string jobName, int fpsYear, string requestedBy, CancellationToken ct = default)
        {
            var jobId = await _repository.GetJobIdByNameAsync(jobName, ct)
                ?? throw new BusinessValidationErrorException([
                    new($"Job name '{jobName}' is not a registered Bulk Rates job.", "INVALID_JOB_NAME")]);

            var canInitiate = await _repository.CanInitiateRequestAsync(jobName, ct);
            if (!canInitiate)
                throw new BusinessValidationErrorException([
                    new($"An active {jobName} request already exists. Complete, reject, " +
                        "or cancel it before creating a new one.", "ACTIVE_REQUEST_EXISTS")]);

            var yearStatus = await _repository.GetFpsYearStatusAsync(fpsYear, ct);
            if (yearStatus is null)
                throw new BusinessValidationErrorException([
                    new($"FPS year {fpsYear} does not exist.", "INVALID_FPS_YEAR")]);

            var requiredStatus = BulkRatesJobNames.RequiredYearStatus(jobName);
            if (!string.Equals(yearStatus, requiredStatus, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"{jobName} requests can only be created for the {requiredStatus} FPS year. " +
                        $"FPS year {fpsYear} is currently {yearStatus}.", "INVALID_YEAR_STATUS_FOR_JOB")]);

            var initiatedStatusId = await _repository.GetStatusIdByNameAsync(jobId, StatusInitiated, ct)
                ?? throw new InvalidOperationException($"Status '{StatusInitiated}' not found for job '{jobName}'.");

            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var entry = await _repository.CreateRequestAsync(
                jobQueueId, jobExecutionId, jobId, initiatedStatusId,
                requestedBy, now, fpsYear, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Request created for FPS year {fpsYear} ({jobName}).",
                requestedBy, ct);

            _logger.LogInformation(
                "[BulkRates.RequestCreated] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor}",
                jobQueueId, jobName, fpsYear, requestedBy);

            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── Upload file ───────────────────────────────────────────────────────────

        public async Task<BulkRatesUploadResultDto> UploadFileAsync(
            Guid jobExecutionId, byte[] fileBytes, string filename,
            string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            // Initiator-only upload restriction.
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // drive the whole workflow during testing. Restore this check before release.

            // if Rejected, auto-transition back to Initiated before replacing staging
            if (string.Equals(entry.Status, StatusRejected, StringComparison.OrdinalIgnoreCase))
            {
                var initiatedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusInitiated, ct)
                    ?? throw new InvalidOperationException($"Status '{StatusInitiated}' not found for job {entry.JobId}.");
                var rejectedStatusId = entry.StatusId;

                await _repository.TransitionStatusAsync(jobQueueId, rejectedStatusId, initiatedStatusId, ct);
                await _repository.WriteJobQueueLogAsync(jobQueueId, "Request re-opened for correction via re-upload.", requestedBy, ct);
                _logger.LogInformation(
                    "[BulkRates.RequestReopened] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                    jobQueueId, entry.JobName, entry.FpsYear, requestedBy, StatusRejected, StatusInitiated);
                entry.StatusId = initiatedStatusId;
                entry.Status = StatusInitiated;
            }

            if (!string.Equals(entry.Status, StatusInitiated, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"File upload is only permitted when the request is in '{StatusInitiated}' status. Current status: {entry.Status}.", "INVALID_STATUS_FOR_UPLOAD")]);

            // Step 1: Parse workbook in-memory (no side effects)
            var parseResult = _parser.Parse(fileBytes, filename, entry.JobName, entry.JobQueueId);

            // Step 2: Envelope check — validate download version before any external write
            if (BulkRatesJobCapabilities.RequiresDownloadVersion(entry.JobName))
            {
                int? carriedVersion = parseResult.WorkbookMetadata.TryGetValue(
                        BulkRatesDownloadMetadataKeys.DownloadVersion, out var carriedVersionText)
                    && int.TryParse(carriedVersionText, out var parsedVersion)
                    ? parsedVersion
                    : null;

                if (carriedVersion is null || entry.ActiveDownloadVersion is null || carriedVersion != entry.ActiveDownloadVersion)
                    throw new BusinessValidationErrorException([
                        new("The uploaded workbook's download version does not match this request's active download. " +
                            "Download the current workbook and upload it without editing the hidden metadata.", "STALE_DOWNLOAD_VERSION")]);
            }

            // Compute SHA-256 and upload version — needed for S3 key before any DB writes
            var checksum = ComputeSha256(fileBytes);
            var newVersion = (entry.UploadVersion ?? 0) + 1;

            // Step 3: Persist original workbook bytes to S3
            var bucket = _configuration["S3Storage:BucketName"]
                ?? throw new InvalidOperationException("S3Storage:BucketName is not configured.");

            var safeFilename = SanitizeS3Filename(filename);
            var folderPath = $"FPS{entry.FpsYear}/BulkRates/{entry.JobName}/{jobQueueId}/v{newVersion}";

            S3UploadResult s3Result;
            using (var stream = new MemoryStream(fileBytes))
            {
                s3Result = await _s3StorageService.UploadFileAsync(
                    stream, bucket, folderPath, safeFilename,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ct);
            }

            if (!s3Result.Success)
            {
                _logger.LogError(
                    "[BulkRates.S3UploadFailed] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | UploadVersion={UploadVersion} | ErrorCode={ErrorCode} | Message={Message}",
                    jobQueueId, entry.JobName, entry.FpsYear, requestedBy, newVersion, s3Result.ErrorCode, s3Result.Message);
                throw new InvalidOperationException(
                    $"Failed to retain uploaded workbook in S3 ({s3Result.ErrorCode}: {s3Result.Message}). Upload aborted.");
            }

            // Step 4: Persist S3 object key — if this fails the S3 object is intentionally retained
            var s3ObjectKey = s3Result.ObjectKey!;
            try
            {
                await _repository.UpdateS3ObjectKeyAsync(jobQueueId, s3ObjectKey, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[BulkRates.S3KeyPersistFailed] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | UploadVersion={UploadVersion} | Bucket={Bucket} | S3ObjectKey={S3ObjectKey}",
                    jobQueueId, entry.JobName, entry.FpsYear, requestedBy, newVersion, bucket, s3ObjectKey);
                throw;
            }

            // Step 5: Business validation and staging (workbook is now retained in S3) — each
            // process service validates its own type and persists its own staging rows; no
            // separate ReplaceStaging*Async dispatch survives here.
            var validationResult = entry.JobName switch
            {
                BulkRatesJobNames.Fec => await _testService.ProcessUploadAsync(
                    parseResult, entry.FpsYear, newVersion, entry.ActiveDownloadVersion, ct),
                BulkRatesJobNames.Staff => await _staffService.ProcessUploadAsync(
                    parseResult, entry.FpsYear, newVersion, ct),
                BulkRatesJobNames.Animal => await _animalService.ProcessUploadAsync(
                    parseResult, entry.FpsYear, newVersion, ct),
                _ => throw new BusinessValidationErrorException([
                    new($"Unknown job: {entry.JobName}", "UNKNOWN_JOB")])
            };

            await _repository.ReplaceValidationErrorsAsync(jobQueueId, validationResult.Errors, ct);

            var counts = validationResult.RowCounts;
            await _repository.UpdateUploadMetadataAsync(
                jobQueueId, filename, checksum, newVersion, DateTime.UtcNow,
                JsonSerializer.Serialize(counts, JsonOptions), ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"File uploaded (v{newVersion}): {filename}. S3: {s3ObjectKey}. Rows: {counts.Total} total, {counts.Invalid} invalid, {counts.Insert} insert, {counts.Update} update, {counts.Unchanged} unchanged.",
                requestedBy, ct);

            _logger.LogInformation(
                "[BulkRates.FileUploaded] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | UploadVersion={UploadVersion} | S3ObjectKey={S3ObjectKey} | TotalRows={TotalRows} | InvalidRows={InvalidRows}",
                jobQueueId, entry.JobName, entry.FpsYear, requestedBy, newVersion, s3ObjectKey, counts.Total, counts.Invalid);

            return new BulkRatesUploadResultDto
            {
                JobQueueId = jobQueueId,
                Status = StatusInitiated,
                UploadVersion = newVersion,
                Filename = filename,
                RowCounts = ToDto(counts),
                ValidationErrors = ToDto(validationResult.Errors)
            };
        }

        /// <summary>
        /// Strips characters that are illegal or problematic in S3 object key path segments.
        /// Preserves the file extension; replaces path separators and control characters with underscores.
        /// </summary>
        private static string SanitizeS3Filename(string filename)
        {
            // Split on both separators so backslash paths are handled correctly on Linux too
            var basename = filename.Split('/', '\\').LastOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? filename;
            var sanitized = Regex.Replace(basename, @"[/\\?#%\x00-\x1f]", "_");
            return string.IsNullOrWhiteSpace(sanitized) ? "upload" : sanitized;
        }

        // ── Get validation results ──────────────────────────────────────────────

        public async Task<BulkRatesUploadResultDto> GetValidationResultsAsync(
            Guid jobExecutionId, string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            // Initiator-only view restriction.
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // drive the whole workflow during testing. Restore this check before release.

            var errors = await _repository.GetValidationErrorsAsync(jobQueueId, ct);
            var metadata = BuildUploadMetadata(entry);

            return new BulkRatesUploadResultDto
            {
                JobQueueId = jobQueueId,
                Status = entry.Status,
                UploadVersion = metadata?.UploadVersion ?? 0,
                Filename = metadata?.Filename,
                RowCounts = ToDto(metadata?.RowCounts ?? new()),
                ValidationErrors = ToDto(errors)
            };
        }

        // ── Release for approval ────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> ReleaseForApprovalAsync(
            Guid jobExecutionId, string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            // Initiator-only release restriction.
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // drive the whole workflow during testing. Restore this check before release.

            RequireStatus(entry, StatusInitiated, "release for approval");

            // Verify upload metadata (checksum) exists
            if (entry.UploadChecksumSha256 == null)
                throw new BusinessValidationErrorException([
                    new("No file has been uploaded for this request. Upload a valid file before releasing.", "NO_UPLOAD")]);
            // BuildUploadMetadata only returns null when both UploadChecksumSha256 and
            // UploadFilename are null — already ruled out by the check above.
            var metadata = BuildUploadMetadata(entry)
                ?? throw new InvalidOperationException("Upload metadata missing despite checksum present.");

            // A checksum can exist for a file that parsed to zero data rows — releasing that
            // would approve/run a no-op bulk update, which makes no business sense.
            if (metadata.RowCounts.Total == 0)
                throw new BusinessValidationErrorException([
                    new("The uploaded file contains no data rows. Upload a file with at least one row before releasing.", "NO_ROWS")]);

            // All blocking errors must be resolved
            var errors = await _repository.GetValidationErrorsAsync(jobQueueId, ct);
            var blockingCount = errors.Count(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase));
            if (blockingCount > 0)
                throw new BusinessValidationErrorException([
                    new($"Cannot release: {blockingCount} blocking validation error(s) must be corrected first.", "BLOCKING_ERRORS")]);

            // Re-run validation against the currently staged rows and *current* live/reference
            // data — not the errors recorded at upload time — because live data (a project, a
            // capability, another request's FEC change, or a live Staff/Animal row) can drift
            // between upload and release. Each process service owns its own complete unit here:
            // load current/staged state, revalidate, throw BusinessValidationErrorException on
            // blocking errors, and persist the frozen calculated actions — nothing left for this
            // method to check or write itself.
            if (string.Equals(entry.JobName, BulkRatesJobNames.Fec, StringComparison.OrdinalIgnoreCase))
            {
                await _testService.PrepareForReleaseAsync(
                    jobQueueId, entry.FpsYear, entry.UploadVersion!.Value, entry.ActiveDownloadVersion, ct);
            }
            else if (string.Equals(entry.JobName, BulkRatesJobNames.Staff, StringComparison.OrdinalIgnoreCase))
            {
                await _staffService.PrepareForReleaseAsync(jobQueueId, entry.FpsYear, ct);
            }
            else if (string.Equals(entry.JobName, BulkRatesJobNames.Animal, StringComparison.OrdinalIgnoreCase))
            {
                await _animalService.PrepareForReleaseAsync(jobQueueId, entry.FpsYear, ct);
            }

            var initiatedStatusId = entry.StatusId;
            var releasedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusReleasedForApproval, ct)
                ?? throw new InvalidOperationException($"Status '{StatusReleasedForApproval}' not found.");

            await _repository.TransitionStatusAsync(jobQueueId, initiatedStatusId, releasedStatusId, ct);
            await _repository.WriteJobQueueLogAsync(jobQueueId, "Request released for approval.", requestedBy, ct);

            try
            {
                await _notificationService.NotifyAsync(
                    BulkRatesNotificationEvent.ReleasedForApproval,
                    new BulkRatesNotificationContext
                    {
                        JobQueueId = jobQueueId,
                        JobName = entry.JobName,
                        FpsYear = entry.FpsYear,
                        RequestedBy = entry.RequestedBy,
                        RowCounts = metadata.RowCounts
                    }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BulkRates.ReleasedForApproval] Notification failed. State transition is preserved. JobQueueId={JobQueueId}", jobQueueId);
            }

            _logger.LogInformation(
                "[BulkRates.ReleasedForApproval] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, requestedBy, StatusInitiated, StatusReleasedForApproval);

            entry.StatusId = releasedStatusId;
            entry.Status = StatusReleasedForApproval;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── Approve ──────────────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> ApproveAsync(
            Guid jobExecutionId, string approvedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            RequireStatus(entry, StatusReleasedForApproval, "approve");

            // Maker-checker — approver must differ from initiator.
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // self-approve during testing. Restore this check before release.

            // Verify checksum is stored (immutability of frozen upload)
            if (entry.UploadChecksumSha256 == null)
                throw new BusinessValidationErrorException([
                    new("Upload metadata is missing. The request cannot be approved.", "MISSING_CHECKSUM")]);

            var releasedStatusId = entry.StatusId;
            var approvedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusApproved, ct)
                ?? throw new InvalidOperationException($"Status '{StatusApproved}' not found.");

            var now = DateTime.UtcNow;

            await _repository.SetApprovalAsync(
                jobQueueId, entry.JobExecutionId,
                approvedBy, now,
                approvedBy, now,
                approvedStatusId, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId, "Request approved.", approvedBy, ct);

            // Publish EventBridge trigger
            var eventDetail = BuildApprovalEvent(entry);
            await _eventPublisherService.PublishAsync(eventDetail, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId, "Processing has been triggered.", approvedBy, ct);

            _logger.LogInformation(
                "[BulkRates.Approved] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | JobExecutionId={JobExecutionId} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, approvedBy, entry.JobExecutionId, StatusReleasedForApproval, StatusApproved);

            try
            {
                await _notificationService.NotifyAsync(
                    BulkRatesNotificationEvent.Approved,
                    new BulkRatesNotificationContext
                    {
                        JobQueueId  = jobQueueId,
                        JobName     = entry.JobName,
                        FpsYear     = entry.FpsYear,
                        RequestedBy = entry.RequestedBy,
                        ApprovedBy  = approvedBy
                    }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BulkRates.Approved] Notification failed; approval state is preserved. JobQueueId={JobQueueId}", jobQueueId);
            }

            entry.ApprovedBy = approvedBy;
            entry.ApprovedAtUtc = now;
            entry.StatusId = approvedStatusId;
            entry.Status = StatusApproved;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── Reject ───────────────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> RejectAsync(
            Guid jobExecutionId, string rejectedBy, string reason, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessValidationErrorException([
                    new("Rejection reason is mandatory.", "REASON_REQUIRED")]);

            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            RequireStatus(entry, StatusReleasedForApproval, "reject");

            // Approver must differ from initiator (same maker-checker rule applies for rejection).
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // self-reject during testing. Restore this check before release.

            var releasedStatusId = entry.StatusId;
            var rejectedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusRejected, ct)
                ?? throw new InvalidOperationException($"Status '{StatusRejected}' not found.");

            var now = DateTime.UtcNow;
            await _repository.SetRejectionAsync(
                jobQueueId, rejectedBy, now, reason, rejectedStatusId, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId, $"Request rejected. Reason: {reason}", rejectedBy, ct);

            try
            {
                await _notificationService.NotifyAsync(
                    BulkRatesNotificationEvent.Rejected,
                    new BulkRatesNotificationContext
                    {
                        JobQueueId = jobQueueId,
                        JobName = entry.JobName,
                        FpsYear = entry.FpsYear,
                        RequestedBy = entry.RequestedBy,
                        Reason = reason
                    }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BulkRates.Rejected] Notification failed. State transition is preserved. JobQueueId={JobQueueId}", jobQueueId);
            }

            _logger.LogInformation(
                "[BulkRates.Rejected] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, rejectedBy, StatusReleasedForApproval, StatusRejected);

            entry.RejectedBy = rejectedBy;
            entry.RejectionReason = reason;
            entry.StatusId = rejectedStatusId;
            entry.Status = StatusRejected;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── Cancel ───────────────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> CancelAsync(
            Guid jobExecutionId, string cancelledBy, string? reason, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            // Initiator-only cancel restriction.
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // drive the whole workflow during testing. Restore this check before release.

            if (!string.Equals(entry.Status, StatusInitiated, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Status, StatusRejected, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Status, StatusFailed, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"Cancellation is only permitted for '{StatusInitiated}', '{StatusRejected}', or '{StatusFailed}' requests. Current status: {entry.Status}.", "INVALID_STATUS_FOR_CANCEL")]);

            var cancelledStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusCancelled, ct)
                ?? throw new InvalidOperationException($"Status '{StatusCancelled}' not found.");

            var now = DateTime.UtcNow;

            // Atomically: set Cancelled + delete all staging rows for this request
            await _repository.CancelAndClearStagingAsync(
                jobQueueId, entry.JobName,
                cancelledBy, now, reason, cancelledStatusId, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                string.IsNullOrWhiteSpace(reason)
                    ? "Request cancelled by initiator."
                    : $"Request cancelled by initiator. Reason: {reason}",
                cancelledBy, ct);

            try
            {
                await _notificationService.NotifyAsync(
                    BulkRatesNotificationEvent.Cancelled,
                    new BulkRatesNotificationContext
                    {
                        JobQueueId = jobQueueId,
                        JobName = entry.JobName,
                        FpsYear = entry.FpsYear,
                        RequestedBy = entry.RequestedBy,
                        Reason = reason
                    }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BulkRates.Cancelled] Notification failed. State transition is preserved. JobQueueId={JobQueueId}", jobQueueId);
            }

            _logger.LogInformation(
                "[BulkRates.Cancelled] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, cancelledBy, entry.Status, StatusCancelled);

            entry.CancelledBy = cancelledBy;
            entry.CancelledAtUtc = now;
            entry.CancellationReason = reason;
            entry.StatusId = cancelledStatusId;
            entry.Status = StatusCancelled;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── Query ────────────────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto?> GetRequestAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await _repository.GetRequestAsync(jobExecutionId, ct);
            if (entry == null) return null;
            return await BuildRequestDtoAsync(entry, ct);
        }

        public async Task<PaginatedResult<BulkRatesQueueEntryDto>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status,
            QueryParameters<string> query, CancellationToken ct = default)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            var paged = await _repository.GetRequestsAsync(
                jobName, fpsYear, status, page, pageSize, query.SortBy, query.Descending, ct);

            return new PaginatedResult<BulkRatesQueueEntryDto>
            {
                Data = paged.Data.Select(ToDto).ToList(),
                PaginationData = new PaginationDto
                {
                    PageNumber = paged.PaginationData.PageNumber,
                    PageSize = paged.PaginationData.PageSize,
                    TotalPages = paged.PaginationData.TotalPages,
                    TotalRecords = paged.PaginationData.TotalRecords
                }
            };
        }

        public Task<bool> CanInitiateRequestAsync(string jobName, CancellationToken ct = default) =>
            _repository.CanInitiateRequestAsync(jobName, ct);

        // ── Export ───────────────────────────────────────────────────────────────

        public async Task<byte[]> ExportFecTestDataAsync(int fpsYear, CancellationToken ct = default)
            => await _testService.ExportTestDataAsync(fpsYear, ct);

        // ── Request-scoped download, atomic with snapshot capture ──────────────────

        public async Task<byte[]> DownloadFecTestDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            RequireDownloadableStatus(entry);

            return await _testService.DownloadTestDataAsync(entry, ct);
        }

        public async Task<byte[]> ExportStaffTestDataAsync(int fpsYear, CancellationToken ct = default)
            => await _staffService.ExportTestDataAsync(fpsYear, ct);

        public async Task<byte[]> ExportAnimalTestDataAsync(int fpsYear, CancellationToken ct = default)
            => await _animalService.ExportTestDataAsync(fpsYear, ct);

        // ── Request-scoped Staff/Animal downloads, parity with
        //    DownloadFecTestDataAsync ─────────────────────────────────────────────────

        public async Task<byte[]> DownloadStaffTestDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            RequireJobName(entry, BulkRatesJobNames.Staff, "download Staff test rates");
            RequireDownloadableStatus(entry);

            return await _staffService.DownloadTestDataAsync(entry, ct);
        }

        public async Task<byte[]> DownloadAnimalTestDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            RequireJobName(entry, BulkRatesJobNames.Animal, "download Animal test rates");
            RequireDownloadableStatus(entry);

            return await _animalService.DownloadTestDataAsync(entry, ct);
        }

        // ── Staging grid (Detail page — "FEC Data (Staging)" / "Agrup Details") ────

        public async Task<BulkRatesStagingDataDto> GetStagingDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);

            // No file uploaded yet — there is nothing staged to diff against live data, so every
            // live row would otherwise look "Deleted"/"Not Found". Return no rows until an upload exists.
            if (entry.UploadChecksumSha256 == null)
                return new BulkRatesStagingDataDto();

            if (string.Equals(entry.JobName, BulkRatesJobNames.Fec, StringComparison.OrdinalIgnoreCase))
                return await _testService.GetStagingDataAsync(entry, ct);

            if (string.Equals(entry.JobName, BulkRatesJobNames.Staff, StringComparison.OrdinalIgnoreCase))
                return await _staffService.GetStagingDataAsync(entry, ct);

            if (string.Equals(entry.JobName, BulkRatesJobNames.Animal, StringComparison.OrdinalIgnoreCase))
                return await _animalService.GetStagingDataAsync(entry, ct);

            return new BulkRatesStagingDataDto();
        }

        public async Task<byte[]> ExportStagingDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);

            // Mirrors GetStagingDataAsync's job-type dispatch, with one legacy quirk preserved
            // exactly: this method predates Staff/Animal staging support and was never given an
            // explicit "unrecognized job name" branch — FEC/AGRUP is the fallback for anything
            // that isn't Staff or Animal, not gated on entry.JobName actually being Fec.
            if (string.Equals(entry.JobName, BulkRatesJobNames.Staff, StringComparison.OrdinalIgnoreCase))
                return await _staffService.ExportStagingDataAsync(entry.JobQueueId, ct);

            if (string.Equals(entry.JobName, BulkRatesJobNames.Animal, StringComparison.OrdinalIgnoreCase))
                return await _animalService.ExportStagingDataAsync(entry.JobQueueId, ct);

            return await _testService.ExportStagingDataAsync(entry.JobQueueId, ct);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private async Task<BulkRatesQueueRow> RequireRequestAsync(Guid jobExecutionId, CancellationToken ct)
        {
            var entry = await _repository.GetRequestAsync(jobExecutionId, ct);
            if (entry == null)
                throw new BusinessValidationErrorException([
                    new($"Bulk Rates request with JobExecutionId {jobExecutionId} not found.", "NOT_FOUND")]);
            return entry;
        }

        private static void RequireStatus(BulkRatesQueueRow entry, string expectedStatus, string action)
        {
            if (!string.Equals(entry.Status, expectedStatus, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"Cannot {action}: request must be in '{expectedStatus}' status. Current status: {entry.Status}.", "INVALID_STATUS_TRANSITION")]);
        }

        private static EventDetail BuildApprovalEvent(BulkRatesQueueRow entry) => new()
        {
            JobExecutionId = entry.JobExecutionId.ToString(),
            JobName = entry.JobName,
            RunMode = "Manual",
            RequestedBy = entry.RequestedBy,
            RequestedAtUtc = entry.RequestedAtUtc,
            ParametersJson = $"{{\"targetFpsYear\":{entry.FpsYear}}}"
        };

        /// <summary>
        /// §6 route-level safety requirement, enforced here at the service
        /// layer rather than deferred to routing: a Staff endpoint must not generate an
        /// Animal workbook, or vice versa, since GetStaffRowsForExportAsync/GetAnimalRowsForExportAsync
        /// key only on FpsYear, not JobQueueId — without this guard, calling
        /// DownloadStaffTestDataAsync with an Animal request's jobExecutionId would silently
        /// snapshot unrelated Staff data under that Animal request's JobQueueId.
        /// </summary>
        private static void RequireJobName(BulkRatesQueueRow entry, string expectedJobName, string action)
        {
            if (!string.Equals(entry.JobName, expectedJobName, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"Cannot {action}: request JobName is '{entry.JobName}', expected '{expectedJobName}'.", "WRONG_JOB_TYPE")]);
        }

        /// <summary>
        /// A new download is only allowed while the request is still
        /// editable — Initiated, or Rejected (uploading from Rejected already auto-transitions
        /// to Initiated, so there is no separate "editable Rejected" status to check).
        /// </summary>
        private static void RequireDownloadableStatus(BulkRatesQueueRow entry)
        {
            if (entry.Status is not (StatusInitiated or StatusRejected))
                throw new BusinessValidationErrorException([
                    new($"Cannot download: request must be in '{StatusInitiated}' or '{StatusRejected}' status. Current status: {entry.Status}.", "INVALID_STATUS_TRANSITION")]);
        }

        private async Task<BulkRatesRequestDto> BuildRequestDtoAsync(BulkRatesQueueRow entry, CancellationToken ct)
        {
            var logs = await _repository.GetJobQueueLogsAsync(entry.JobQueueId, ct);
            var errors = await _repository.GetValidationErrorsAsync(entry.JobQueueId, ct);
            var metadata = BuildUploadMetadata(entry);

            return new BulkRatesRequestDto
            {
                Entry = ToDto(entry),
                UploadMetadata = ToDto(metadata),
                Log = ToDto(logs),
                ErrorCount = errors.Count(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase)),
                WarningCount = errors.Count(e => string.Equals(e.Severity, "Warning", StringComparison.OrdinalIgnoreCase))
            };
        }

        private static BulkRatesUploadMetadata? BuildUploadMetadata(BulkRatesQueueRow entry)
        {
            if (entry.UploadChecksumSha256 == null && entry.UploadFilename == null)
                return null;

            return new BulkRatesUploadMetadata
            {
                Filename = entry.UploadFilename,
                ChecksumSha256 = entry.UploadChecksumSha256,
                UploadVersion = entry.UploadVersion ?? 0,
                ValidationCompletedAtUtc = entry.UploadValidatedAtUtc,
                RowCounts = DeserializeRowCounts(entry.UploadRowCountsJson)
            };
        }

        private static BulkRatesRowCounts DeserializeRowCounts(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try { return JsonSerializer.Deserialize<BulkRatesRowCounts>(json, JsonOptions) ?? new(); }
            catch { return new(); }
        }

        // ── Core entity → API Dto mapping (API-boundary correction: the JSON contract
        // must never serialize Core.Entities types directly) ───────────────────────

        private static BulkRatesQueueEntryDto ToDto(BulkRatesQueueRow entry) => new()
        {
            JobQueueId = entry.JobQueueId,
            JobId = entry.JobId,
            JobName = entry.JobName,
            StatusId = entry.StatusId,
            Status = entry.Status,
            JobExecutionId = entry.JobExecutionId,
            RequestedBy = entry.RequestedBy,
            RequestedAtUtc = entry.RequestedAtUtc,
            FpsYear = entry.FpsYear,
            UploadFilename = entry.UploadFilename,
            UploadChecksumSha256 = entry.UploadChecksumSha256,
            UploadVersion = entry.UploadVersion,
            UploadValidatedAtUtc = entry.UploadValidatedAtUtc,
            UploadRowCountsJson = entry.UploadRowCountsJson,
            ApprovedBy = entry.ApprovedBy,
            ApprovedAtUtc = entry.ApprovedAtUtc,
            RejectedBy = entry.RejectedBy,
            RejectedAtUtc = entry.RejectedAtUtc,
            RejectionReason = entry.RejectionReason,
            CancelledBy = entry.CancelledBy,
            CancelledAtUtc = entry.CancelledAtUtc,
            CancellationReason = entry.CancellationReason,
            TriggeredBy = entry.TriggeredBy,
            TriggeredAtUtc = entry.TriggeredAtUtc,
            StartDateTime = entry.StartDateTime,
            EndDateTime = entry.EndDateTime,
            ErrorMessage = entry.ErrorMessage,
            FailureReason = entry.FailureReason,
            ActiveDownloadVersion = entry.ActiveDownloadVersion,
            S3ObjectKey = entry.S3ObjectKey,
        };

        private static BulkRatesUploadMetadataDto? ToDto(BulkRatesUploadMetadata? metadata) => metadata is null ? null : new BulkRatesUploadMetadataDto
        {
            Filename = metadata.Filename,
            ChecksumSha256 = metadata.ChecksumSha256,
            UploadVersion = metadata.UploadVersion,
            ValidationCompletedAtUtc = metadata.ValidationCompletedAtUtc,
            RowCounts = ToDto(metadata.RowCounts),
        };

        private static BulkRatesRowCountsDto ToDto(BulkRatesRowCounts counts) => new()
        {
            Total = counts.Total,
            Valid = counts.Valid,
            Invalid = counts.Invalid,
            Insert = counts.Insert,
            Update = counts.Update,
            Unchanged = counts.Unchanged,
            FecTotal = counts.FecTotal,
            FecInsert = counts.FecInsert,
            FecUpdate = counts.FecUpdate,
            FecUnchanged = counts.FecUnchanged,
            FecInvalid = counts.FecInvalid,
            AgrupTotal = counts.AgrupTotal,
            AgrupInsert = counts.AgrupInsert,
            AgrupUpdate = counts.AgrupUpdate,
            AgrupUnchanged = counts.AgrupUnchanged,
            AgrupInvalid = counts.AgrupInvalid,
        };

        // BatchJobQueueLog is the shared EF entity for fps.job_queue_log — BulkRates reuses it
        // directly (see BulkRatesRepository.GetJobQueueLogsAsync) rather than maintaining its
        // own near-duplicate raw-ADO type. This mapping is where the persistence-oriented
        // column names become the consumer-friendly names BulkRatesQueueLogDto already exposes.
        private static BulkRatesQueueLogDto ToDto(BatchJobQueueLog log) => new()
        {
            LogId = log.JobqueueLogId,
            JobQueueId = log.JobqueueId,
            Note = log.Note ?? string.Empty,
            Actor = log.PerformedBy,
            CreatedAtUtc = log.LogTime,
        };

        private static IReadOnlyList<BulkRatesQueueLogDto> ToDto(IReadOnlyList<BatchJobQueueLog> logs) =>
            logs.Select(ToDto).ToList();

        private static BulkRatesValidationErrorDto ToDto(StagingValidationError error) => new()
        {
            Id = error.Id,
            JobQueueId = error.JobQueueId,
            UploadVersion = error.UploadVersion,
            SourceRowNumber = error.SourceRowNumber,
            FieldName = error.FieldName,
            ValidationCode = error.ValidationCode,
            Severity = error.Severity,
            ValidationMessage = error.ValidationMessage,
            SheetName = error.SheetName,
            TestCode = error.TestCode,
            Buyer = error.Buyer,
            CurrentValue = error.CurrentValue,
            ExpectedValue = error.ExpectedValue,
            IsRequestLevel = error.IsRequestLevel,
        };

        private static IReadOnlyList<BulkRatesValidationErrorDto> ToDto(IReadOnlyList<StagingValidationError> errors) =>
            errors.Select(ToDto).ToList();

        private static string ComputeSha256(byte[] data)
        {
            var hash = SHA256.HashData(data);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }
}
