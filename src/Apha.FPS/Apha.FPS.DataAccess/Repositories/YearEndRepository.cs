using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class YearEndRepository : BaseRepository, IYearEndRepository
    {
        // Matches YearEndService's own YearEndDataSetupJobName constant (and
        // YearEndStagingRepository's). GetInitiatedDataSetupJobExecutionIdAsync is a Data-Setup-only
        // concept (it resolves the request the Confirm workflow is editing) — hardcoded here rather
        // than threaded as a parameter, so the caller can't ask it for a different job's active id.
        private const string YearEndDataSetupJobName = "YearEnd-DataSetup";

        // Same reasoning as YearEndDataSetupJobName above, for CutOver's own resolve-by-JobExecutionId methods.
        private const string YearEndCutOverJobName = "YearEnd-CutOver";

        // Only used by the DataSetup Approve/Reject path (Workstream 6), specifically for Reject's
        // staging deletion. Relies on both repositories resolving the same scoped FpsDbContext instance
        // (both AddScoped, both take FpsDbContext directly, not a factory) so DeleteStagingAsync's
        // SaveChangesAsync joins the transaction opened below rather than committing separately.
        private readonly IYearEndStagingRepository _yearEndStagingRepository;

        // Resolves job_queue.fpsyear ("current/Open year") at Initiation from the authoritative
        // tblyearmaster row, not from any ambient per-request context (X-FPS-Year reflects whichever
        // year a UI session happens to be browsing - an administrative operation like Year End must
        // not change meaning based on that).
        private readonly IYearMasterRepository _yearMasterRepository;

        public YearEndRepository(FpsDbContext context, IYearEndStagingRepository yearEndStagingRepository, IYearMasterRepository yearMasterRepository) : base(context)
        {
            _yearEndStagingRepository = yearEndStagingRepository ?? throw new ArgumentNullException(nameof(yearEndStagingRepository));
            _yearMasterRepository = yearMasterRepository ?? throw new ArgumentNullException(nameof(yearMasterRepository));
        }

        public async Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName)
        {
            IQueryable<BatchJobHistory> jobHistoriesQuery =
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower()
                select new BatchJobHistory
                {
                    JobId = jm.JobId,
                    JobName = jm.JobName,
                    JobExecutionId = jq.JobExecutionId,
                    RequestedBy = jq.RequestedBy,
                    StartDateTime = jq.StartDateTime,
                    EndDateTime = jq.EndDateTime,
                    ErrorMessage = jq.ErrorMessage,
                    Status = js.Status
                };

            jobHistoriesQuery = (IQueryable<BatchJobHistory>)ApplySorting(jobHistoriesQuery, query.SortBy?.ToLower(), query.Descending);
            var result = await jobHistoriesQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName)
        {
            bool hasNonTerminalRecord = await CanInitiateRequest(jobName);

            return !hasNonTerminalRecord;
        }

        public async Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName)
        {
            bool hasRunningJob = await CanApproveOrRejectRequest(jobName);

            return hasRunningJob;
        }

        public async Task<string> GetYearEndDataSetupRequestInitiatorAsync(string jobName)
        {
            return await GetInitiator(jobName);
        }

        public async Task<Guid?> GetInitiatedDataSetupJobExecutionIdAsync()
        {
            // IgnoreQueryFilters: BatchJobQueue carries a global HasQueryFilter(e => e.FpsYear ==
            // FilterFpsYear) — this is recovering workflow state ("is there an editable Data Setup
            // request, anywhere"), not a year-scoped listing, and must not depend on the caller's
            // ambient X-FPS-Year header matching whatever FpsYear the request's own row carries.
            //
            // (Guid?) cast: without it, SingleOrDefaultAsync over a non-nullable Guid projection
            // returns Guid.Empty for zero rows, not null — silently breaking the "no request" contract.
            //
            // SingleOrDefaultAsync, not FirstOrDefaultAsync + OrderByDescending: at most one Initiated
            // Data Setup request should ever exist system-wide (CanInitiateRequest's own invariant). If
            // that's ever violated, this must fail loudly rather than silently picking one.
            return await (
                from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                join jm in _context.BatchJobs.AsNoTracking() on jq.JobId equals jm.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == YearEndDataSetupJobName.ToLower()
                   && js.Status.ToLower() == "initiated"
                select (Guid?)jq.JobExecutionId
            ).SingleOrDefaultAsync();
        }

        public async Task<BatchJobQueue> EnqueueDataSetupInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note, int targetFpsYear)
        {
            return await EnqueueInitiationRequest(jobName, requestedBy, correlationId, note, targetFpsYear);
        }

        public async Task<BatchJobQueue> EnqueueDataSetupApprovalBatchJobAsync(Guid jobQueueId, string requestedBy, string note)
        {
            return await EnqueueDataSetupApprovalOrRejectByJobQueueId(jobQueueId, requestedBy, note, false);
        }

        public async Task<BatchJobQueue> EnqueueDataSetupRejectBatchJobAsync(Guid jobQueueId, string requestedBy, string note)
        {
            return await EnqueueDataSetupApprovalOrRejectByJobQueueId(jobQueueId, requestedBy, note, true);
        }

        [SuppressMessage("SonarAnalyzer.CSharp", "S4144:MethodsShouldNotHaveIdenticalImplementations",
            Justification = "Intentionally identical: DataSetup and CutOver are distinct domain operations that must remain independently named for clarity and maintainability.")]
        public async Task<bool> CanInitiateYearEndCutOverRequestAsync(string jobName)
        {
            bool hasNonTerminalRecord = await CanInitiateRequest(jobName);

            return !hasNonTerminalRecord;
        }

        [SuppressMessage("SonarAnalyzer.CSharp", "S4144:MethodsShouldNotHaveIdenticalImplementations",
            Justification = "Intentionally identical: DataSetup and CutOver are distinct domain operations that must remain independently named for clarity and maintainability.")]
        public async Task<bool> CanApproveOrRejectYearEndCutOverRequestAsync(string jobName)
        {
            bool hasRunningJob = await CanApproveOrRejectRequest(jobName);

            return hasRunningJob;
        }

        [SuppressMessage("SonarAnalyzer.CSharp", "S4144:MethodsShouldNotHaveIdenticalImplementations",
            Justification = "Intentionally identical: DataSetup and CutOver are distinct domain operations that must remain independently named for clarity and maintainability.")]
        public async Task<string> GetYearEndCutOverRequestInitiatorAsync(string jobName)
        {
            return await GetInitiator(jobName);
        }

        public async Task<Guid?> GetInitiatedCutOverJobExecutionIdAsync()
        {
            // Same reasoning as GetInitiatedDataSetupJobExecutionIdAsync above.
            return await (
                from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                join jm in _context.BatchJobs.AsNoTracking() on jq.JobId equals jm.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == YearEndCutOverJobName.ToLower()
                   && js.Status.ToLower() == "initiated"
                select (Guid?)jq.JobExecutionId
            ).SingleOrDefaultAsync();
        }

        public async Task<BatchJobQueue> EnqueueCutOverInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueInitiationRequest(jobName, requestedBy, correlationId, note);
        }

        public async Task<BatchJobQueue> EnqueueCutOverApprovalBatchJobAsync(Guid jobExecutionId, string requestedBy, string note)
        {
            return await EnqueueCutOverApprovalOrRejectByJobExecutionId(jobExecutionId, requestedBy, note, false);
        }

        public async Task<BatchJobQueue> EnqueueCutOverRejectBatchJobAsync(Guid jobExecutionId, string requestedBy, string note)
        {
            return await EnqueueCutOverApprovalOrRejectByJobExecutionId(jobExecutionId, requestedBy, note, true);
        }

        public async Task SetTriggeredMetadataAsync(string jobExecutionId, string triggeredBy)
        {
            if (string.IsNullOrWhiteSpace(jobExecutionId) || !Guid.TryParse(jobExecutionId, out var parsedJobExecutionId))
                throw new ArgumentException("A valid jobExecutionId is required.", nameof(jobExecutionId));

            // Tracked (not AsNoTracking) and without an explicit Update() call: the Approve/Reject
            // call just before this one (EnqueueDataSetupApprovalOrRejectByJobQueueId /
            // EnqueueCutOverApprovalOrRejectByJobExecutionId) already tracked+saved this same row
            // in this scoped FpsDbContext. A second AsNoTracking() query + Update() here would
            // attach a distinct instance for the same key while the first is still tracked, which
            // EF rejects ("already being tracked"). Querying tracked instead lets EF's identity
            // resolution return that same already-tracked instance so mutating it just works.
            // IgnoreQueryFilters for the same reason as every other unique-id lookup in this file -
            // must not depend on the ambient X-FPS-Year header matching this row's FpsYear.
            var queueRow = await _context.BatchJobQueues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(q => q.JobExecutionId == parsedJobExecutionId)
                ?? throw new KeyNotFoundException($"Batch job queue row for JobExecutionId '{jobExecutionId}' was not found.");

            var nowUtc = DateTime.UtcNow;
            queueRow.TriggeredBy = triggeredBy;
            queueRow.TriggeredAtUtc = nowUtc;
            queueRow.UpdatedAt = nowUtc;

            await _context.SaveChangesAsync();
        }

        private async Task<bool> CanInitiateRequest(string jobName)
        {
            // IgnoreQueryFilters: same reasoning as GetInitiatedDataSetupJobExecutionIdAsync above - the
            // "at most one non-terminal request, system-wide" invariant that comment (and the staging
            // simplification design that now depends on it) assumes must not be undermined by scoping
            // this check to the caller's ambient X-FPS-Year. Without this, a second Initiate whose
            // ambient header happens to differ from an existing request's FpsYear would slip past this
            // check entirely.
            //
            // Returns true when no records exist for the job, OR every record is in a terminal status (rejected / failed / cancelled).
            // Returns false when at least one record exists that is NOT in a terminal status.
            return await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower()
                   && js.Status.ToLower() != "rejected"
                   && js.Status.ToLower() != "failed"
                   && js.Status.ToLower() != "cancelled"
                select jq.JobqueueId
            ).AnyAsync();
        }

        private async Task<bool> CanApproveOrRejectRequest(string jobName)
        {
            // IgnoreQueryFilters: same reasoning as CanInitiateRequest above.
            return await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower() && (js.Status.ToLower() == "initiated")
                select jq.JobqueueId
            ).AnyAsync();
        }

        private async Task<string> GetInitiator(string jobName)
        {
            // IgnoreQueryFilters: same reasoning as CanInitiateRequest above.
            var initiator = await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower() && (js.Status.ToLower() == "initiated")
                select jq.RequestedBy
            ).FirstOrDefaultAsync();

            return initiator ?? string.Empty;
        }

        // CutOver's version of EnqueueDataSetupApprovalOrRejectByJobQueueId below, keyed by
        // JobExecutionId instead - no staging to delete on reject.
        private async Task<BatchJobQueue> EnqueueCutOverApprovalOrRejectByJobExecutionId(Guid jobExecutionId, string requestedBy, string note, bool isReject)
        {
            BatchJobQueue queueRow = null!;
            BatchJobStatus jobStatus;

            var jobqueue = await (
                from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                join jm in _context.BatchJobs.AsNoTracking() on jq.JobId equals jm.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jq.JobExecutionId == jobExecutionId
                   && jm.JobName.ToLower() == YearEndCutOverJobName.ToLower()
                   && js.Status.ToLower() == "initiated"
                select new { jq.JobqueueId, jq.JobId }
            ).FirstOrDefaultAsync();

            if (jobqueue == null)
            {
                throw new KeyNotFoundException($"No initiated CutOver request was found for JobExecutionId '{jobExecutionId}'.");
            }

            if (isReject)
            {
                jobStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .Where(s => s.JobId == jobqueue.JobId && s.Status.ToLower() == "rejected")
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Status 'rejected' not found for JobExecutionId '{jobExecutionId}'.");
            }
            else
            {
                jobStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .Where(s => s.JobId == jobqueue.JobId && s.Status.ToLower() == "approved")
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Status 'approved' not found for JobExecutionId '{jobExecutionId}'.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    queueRow = await (
                        from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                        join js in _context.BatchJobStatuses.AsNoTracking()
                            on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                        where jq.JobqueueId == jobqueue.JobqueueId && js.Status.ToLower() == "initiated"
                        select jq
                    ).FirstOrDefaultAsync()
                    ?? throw new KeyNotFoundException($"Batch job queue '{jobqueue.JobqueueId}' is no longer in Initiated status.");

                    //update the status of the job queue entry to "approved" or "rejected"
                    var decidedAtUtc = DateTime.UtcNow;
                    queueRow.StatusId = jobStatus.StatusId;
                    queueRow.RequestedBy = requestedBy;
                    queueRow.RequestedAtUtc = decidedAtUtc;
                    queueRow.StartDateTime = decidedAtUtc;
                    queueRow.ErrorMessage = note;

                    if (isReject)
                    {
                        queueRow.RejectedBy = requestedBy;
                        queueRow.RejectedAtUtc = decidedAtUtc;
                        queueRow.RejectionReason = note;
                    }
                    else
                    {
                        queueRow.ApprovedBy = requestedBy;
                        queueRow.ApprovedAtUtc = decidedAtUtc;
                    }

                    _context.BatchJobQueues.Update(queueRow);

                    BatchJobQueueLog logEntry = BuildJobQueueLogEntry(requestedBy, jobqueue.JobqueueId, note, DateTime.UtcNow, jobStatus.StatusId);
                    _context.BatchJobQueueLogs.Add(logEntry);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return queueRow;
        }

        private async Task<BatchJobQueue> EnqueueDataSetupApprovalOrRejectByJobQueueId(Guid jobQueueId, string requestedBy, string note, bool isReject)
        {
            BatchJobQueue queueRow = null!;
            BatchJobStatus jobStatus;

            // IgnoreQueryFilters: BatchJobQueue carries a global HasQueryFilter(e => e.FpsYear ==
            // FilterFpsYear) - a lookup by unique jobQueueId must not depend on the ambient X-FPS-Year
            // header matching this row's FpsYear (same reasoning as
            // YearEndStagingRepository.ResolveRequestAsync).
            var jobqueue = await (
                from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jq.JobqueueId == jobQueueId && js.Status.ToLower() == "initiated"
                select new { jq.JobqueueId, jq.JobId }
            ).FirstOrDefaultAsync();

            if (jobqueue == null)
            {
                throw new KeyNotFoundException($"No initiated Data Setup request was found for job queue '{jobQueueId}'.");
            }

            if (isReject)
            {
                jobStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .Where(s => s.JobId == jobqueue.JobId && s.Status.ToLower() == "rejected")
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Status 'rejected' not found for job queue '{jobQueueId}'.");
            }
            else
            {
                jobStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .Where(s => s.JobId == jobqueue.JobId && s.Status.ToLower() == "approved")
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Status 'approved' not found for job queue '{jobQueueId}'.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Re-checks Initiated status, not just existence by id.
                    queueRow = await (
                        from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                        join js in _context.BatchJobStatuses.AsNoTracking()
                            on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                        where jq.JobqueueId == jobQueueId && js.Status.ToLower() == "initiated"
                        select jq
                    ).FirstOrDefaultAsync()
                    ?? throw new KeyNotFoundException($"Batch job queue '{jobQueueId}' is no longer in Initiated status.");

                    //update the status of the job queue entry to "approved" or "rejected"
                    var decidedAtUtc = DateTime.UtcNow;
                    queueRow.StatusId = jobStatus.StatusId;
                    queueRow.RequestedBy = requestedBy;
                    queueRow.RequestedAtUtc = decidedAtUtc;
                    queueRow.StartDateTime = decidedAtUtc;
                    queueRow.ErrorMessage = note;

                    if (isReject)
                    {
                        queueRow.RejectedBy = requestedBy;
                        queueRow.RejectedAtUtc = decidedAtUtc;
                        queueRow.RejectionReason = note;
                    }
                    else
                    {
                        queueRow.ApprovedBy = requestedBy;
                        queueRow.ApprovedAtUtc = decidedAtUtc;
                    }

                    _context.BatchJobQueues.Update(queueRow);

                    BatchJobQueueLog logEntry = BuildJobQueueLogEntry(requestedBy, jobQueueId, note, DateTime.UtcNow, jobStatus.StatusId);
                    _context.BatchJobQueueLogs.Add(logEntry);

                    await _context.SaveChangesAsync();

                    if (isReject)
                    {
                        // Deletes the staged Config Value/Month Hours rows in the same transaction as
                        // the status flip, so a Rejected request can never retain editable workflow
                        // data. Staging is a singleton (not jobqueueid-scoped) — there is only ever one
                        // candidate row set, per CanInitiateRequest's invariant. Relies on
                        // _yearEndStagingRepository resolving this same scoped FpsDbContext instance
                        // (see constructor comment) - its SaveChangesAsync joins this ambient
                        // transaction rather than committing separately.
                        await _yearEndStagingRepository.DeleteStagingAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return queueRow;
        }

        private async Task<BatchJobQueue> EnqueueInitiationRequest(string jobName, string requestedBy, string correlationId, string note, int? targetFpsYear = null)
        {
            BatchJobQueue jobQueueEntry = null!;

            var job = await _context.BatchJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobName.ToLower() == jobName.ToLower())
                ?? throw new KeyNotFoundException($"Batch job '{jobName}' was not found.");

            var initiatedStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.JobId == job.JobId && s.Status.ToLower() == "initiated")
                ?? throw new KeyNotFoundException($"Status 'initiated' not found for job '{jobName}'.");

            var openYear = await _yearMasterRepository.GetOpenFpsYearAsync()
                ?? throw new InvalidOperationException("No FPS year in fps.tblyearmaster is currently 'Open' - cannot determine the current year for this Year End request.");

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (string.Equals(jobName, YearEndDataSetupJobName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Defensive: staging is a singleton now (no jobqueueid scoping), so a fresh
                        // Initiate no longer starts with an empty staging state implicitly the way a
                        // brand-new jobqueueid always did. If a previous cycle's staging was somehow
                        // left non-empty (a Worker crash that skipped its own clear, a manual DB
                        // intervention), this Initiate would otherwise silently inherit stale rows from
                        // a completely unrelated prior request. Cheap, and a no-op in the common case.
                        await _yearEndStagingRepository.DeleteStagingAsync();
                    }

                    jobQueueEntry = BuildJobQueueEntry(requestedBy, correlationId, note, job.JobId, initiatedStatus.StatusId, openYear.FpsYear, targetFpsYear);
                    _context.BatchJobQueues.Add(jobQueueEntry);

                    BatchJobQueueLog logEntry = BuildJobQueueLogEntry(jobQueueEntry.RequestedBy, jobQueueEntry.JobqueueId, note, jobQueueEntry.StartDateTime, initiatedStatus.StatusId);
                    _context.BatchJobQueueLogs.Add(logEntry);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return jobQueueEntry;
        }

        private static IQueryable ApplySorting(IQueryable<BatchJobHistory> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderByDescending(e => e.StartDateTime);
            }
            else
            {
                return sortBy switch
                {
                    "jobid" => ApplyOrder(query, i => i.JobId, descending),
                    "jobname" => ApplyOrder(query, i => i.JobName, descending),
                    "jobexecutionid" => ApplyOrder(query, i => i.JobExecutionId, descending),
                    "requestedby" => ApplyOrder(query, i => i.RequestedBy, descending),
                    "startdatetime" => ApplyOrder(query, i => i.StartDateTime, descending),
                    "enddatetime" => ApplyOrder(query, i => i.EndDateTime, descending),
                    "errormessage" => ApplyOrder(query, i => i.ErrorMessage, descending),
                    "status" => ApplyOrder(query, i => i.Status, descending),
                    _ => query.OrderBy(e => e.StartDateTime)
                };
            }
        }

        private static IQueryable ApplyOrder<T>(IQueryable<BatchJobHistory> query, Expression<Func<BatchJobHistory, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static BatchJobQueue BuildJobQueueEntry(string requestedBy, string correlationId, string note, int jobId, int statusId, int contextYear, int? targetFpsYear = null)
        {
            return new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(),
                JobExecutionId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid() : Guid.Parse(correlationId),
                JobId = jobId,
                StatusId = statusId,
                RequestedBy = requestedBy,
                RequestedAtUtc = DateTime.UtcNow,
                StartDateTime = DateTime.UtcNow,
                ErrorMessage = note,
                FpsYear = contextYear,
                TargetFpsYear = targetFpsYear
            };
        }

        private static BatchJobQueueLog BuildJobQueueLogEntry(string requestedBy, Guid jobqueueId, string note, DateTime logtime, int statusId)
        {
            return new BatchJobQueueLog
            {
                JobqueueId = jobqueueId,
                StatusId = statusId,
                PerformedBy = requestedBy,
                LogTime = logtime,
                Note = note
            };
        }
    }
}
