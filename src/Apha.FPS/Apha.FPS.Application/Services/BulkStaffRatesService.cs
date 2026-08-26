using Apha.Common.Utilities.ExcelExport;
using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Owns the Staff Bulk Rates process end to end — upload validation and staging,
    /// release-time revalidation and freeze, and staging/export/download presentation. Staff is
    /// update-only: no insert path, no ZeroRateWithdrawal. Extracted from
    /// <c>StaffAnimalValidationService</c>/<c>BulkRatesValidator</c>/<c>BulkRatesRequestService</c>
    /// per the low-risk phase-wise execution plan, Phases 3-4. Not yet wired into
    /// <c>BulkRatesRequestService</c> or DI.
    ///
    /// <c>LiveStaffRow</c>/<c>ValidationStaffRow</c>/<c>StaffFieldState</c>/
    /// <c>StaffValidationResult</c> below are private nested types — genuinely Staff-only (never
    /// shared with Animal), so safe to duplicate out of the still-standalone
    /// <c>Validation/BulkRates/</c> files. <see cref="Common.BulkRates.StaffAnimalFieldComparer"/>/
    /// <see cref="Common.BulkRates.StaffAnimalCalculatedAction"/>/
    /// <see cref="Common.BulkRates.StaffAnimalValidationVersion"/> are genuinely shared with
    /// <c>BulkAnimalRatesService</c> and now live in <c>Common/BulkRates/</c> (Phase 4). The old
    /// combined <c>StaffAnimalValidationKeys.PcGrade</c> is inlined below as a private
    /// Staff-only helper instead — <c>BulkAnimalRatesService</c> gets its own private
    /// <c>AnimalType</c> helper rather than both continuing to share one file whose two halves
    /// serve different services.
    /// </summary>
    public class BulkStaffRatesService : IBulkStaffRatesService
    {
        private readonly IBulkRatesRepository _repository;
        private readonly IExcelExportService _excelExportService;
        private readonly ILogger<BulkStaffRatesService> _logger;

        public BulkStaffRatesService(
            IBulkRatesRepository repository,
            IExcelExportService excelExportService,
            ILogger<BulkStaffRatesService> logger)
        {
            _repository = repository;
            _excelExportService = excelExportService;
            _logger = logger;
        }

        // ── Upload ───────────────────────────────────────────────────────────────

        public async Task<BulkRatesValidationResult> ProcessUploadAsync(
            BulkRatesParseResult parseResult, int fpsYear, int uploadVersion, CancellationToken ct = default)
        {
            // File-level parse errors become Error-severity validation entries on row 0.
            // Staging is still replaced with whatever rows did parse (§1c) — must not become
            // "parse errors -> return immediately".
            if (parseResult.HasParseErrors)
            {
                var fileErrors = parseResult.ParseErrors.Select(msg => new StagingValidationError
                {
                    JobQueueId = parseResult.JobQueueId,
                    UploadVersion = uploadVersion,
                    SourceRowNumber = 0,
                    FieldName = "file",
                    ValidationCode = "FILE_ERROR",
                    Severity = "Error",
                    ValidationMessage = msg
                }).ToList();

                await _repository.ReplaceStagingStaffAsync(parseResult.JobQueueId, parseResult.StaffRows, ct);

                return new BulkRatesValidationResult { Errors = fileErrors, RowCounts = new() };
            }

            var context = await BuildContextAsync(parseResult.JobQueueId, fpsYear, parseResult.StaffRows, ct);
            var results = ValidateStaff(context);

            var errors = results
                .SelectMany(r => r.Errors)
                .Select(f => BulkRatesValidationFindingMapper.MapFinding(f, parseResult.JobQueueId, uploadVersion))
                .ToList();

            var counts = StaffAnimalRowCountHelper.ComputeRowCounts(parseResult.StaffRows.Count, results.Select(r => r.Action), errors);

            await _repository.ReplaceStagingStaffAsync(parseResult.JobQueueId, parseResult.StaffRows, ct);

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        // ── Release-time re-validation + freeze ─────────────────────────────────────

        public async Task PrepareForReleaseAsync(Guid jobQueueId, int fpsYear, CancellationToken ct = default)
        {
            var stagedRows = await _repository.GetProfitCentreGradeStagingRowsAsync(jobQueueId, ct);
            var context = await BuildContextAsync(jobQueueId, fpsYear, stagedRows, ct);
            var results = ValidateStaff(context);

            var blockingErrors = results
                .SelectMany(r => r.Errors)
                .Where(f => f.Severity == ValidationSeverity.Error)
                .ToList();
            if (blockingErrors.Count > 0)
                throw new BusinessValidationErrorException(blockingErrors
                    .Select(f => new BusinessValidationError(f.Message, f.ValidationCode))
                    .ToList());

            var freezes = results.Select(r => new StaffFreezeEntry(
                r.PcGrade, r.Action,
                r.Source?.PayRate, r.Source?.Npr, r.Source?.Ohr,
                r.Effective?.PayRate, r.Effective?.Npr, r.Effective?.Ohr,
                r.Effective?.ChargeRate)).ToList();

            await _repository.FreezeStaffStagingAsync(jobQueueId, StaffAnimalValidationVersion.Current, freezes, ct);
        }

        // ── Export / download ───────────────────────────────────────────────────────

        public async Task<byte[]> ExportTestDataAsync(int fpsYear, CancellationToken ct = default)
        {
            var staffRows = await _repository.GetStaffRowsForExportAsync(fpsYear, ct);

            _logger.LogInformation(
                "[BulkRates.ExportStaffTestData] FpsYear={FpsYear} | StaffRows={StaffRows}",
                fpsYear, staffRows.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildStaffSheet(staffRows));
        }

        public async Task<byte[]> DownloadTestDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
        {
            var downloadVersion = await _repository.GetNextDownloadVersionAsync(entry.JobQueueId, ct);

            // Steps 1-2, matching FEC/AGRUP's DownloadTestDataAsync exactly: snapshot live data as
            // the new Generating header, in one transaction.
            var liveRows = await _repository.GetStaffRowsForExportAsync(entry.FpsYear, ct);
            await _repository.CreateStaffDownloadSnapshotAsync(entry.JobQueueId, downloadVersion, liveRows, ct);

            try
            {
                // Step 3: generate from the just-persisted snapshot, not a second live query.
                var snapshotRows = await _repository.GetStaffSnapshotRowsAsync(entry.JobQueueId, downloadVersion, ct);

                var metadata = new Dictionary<string, string>
                {
                    [BulkRatesDownloadMetadataKeys.JobQueueId] = entry.JobQueueId.ToString(),
                    [BulkRatesDownloadMetadataKeys.DownloadVersion] = downloadVersion.ToString(),
                };
                var bytes = _excelExportService.ExportToExcelMultiSheet(BuildStaffSheet(snapshotRows), metadata);

                // Step 4: only on success, Ready + active_download_version.
                await _repository.MarkDownloadReadyAsync(entry.JobQueueId, downloadVersion, ct);

                _logger.LogInformation(
                    "[BulkRates.DownloadStaffTestData] JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | StaffRows={StaffRows}",
                    entry.JobQueueId, downloadVersion, snapshotRows.Count);

                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[BulkRates.DownloadStaffTestData] Workbook generation failed after snapshot commit | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion}",
                    entry.JobQueueId, downloadVersion);
                await _repository.MarkDownloadFailedAsync(entry.JobQueueId, downloadVersion, ct);
                throw;
            }
        }

        // ── Staging grid / export ────────────────────────────────────────────────────

        // Staff/Animal are update-only: a staged row whose key doesn't exist live is skipped,
        // never inserted, and a live row absent from the upload is left untouched, never deleted.
        // So unlike FEC/AGRUP, there is no "Inserted"/"Deleted" status here — only "No Change",
        // "Updated", and "Not Found" (surfaced so the initiator can catch a typo'd grade before
        // release, matching exactly what the worker will do). Every staged row is shown.
        public async Task<BulkRatesStagingDataDto> GetStagingDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
        {
            var stagedStaff = await _repository.GetProfitCentreGradeStagingRowsAsync(entry.JobQueueId, ct);
            var liveStaff = await _repository.GetStaffRowsForExportAsync(entry.FpsYear, ct);
            var liveByGrade = liveStaff.ToDictionary(r => r.PcGrade, StringComparer.OrdinalIgnoreCase);

            var rows = new List<BulkRatesStaffStagingRowDto>();
            foreach (var row in stagedStaff)
            {
                if (!liveByGrade.TryGetValue(row.PcGrade, out var live))
                {
                    var newPay = row.PayRate ?? 0m;
                    var newNpr = row.Npr ?? 0m;
                    var newOhr = row.Ohr ?? 0m;
                    rows.Add(new BulkRatesStaffStagingRowDto
                    {
                        Status = "Not Found",
                        PcGrade = row.PcGrade,
                        PayRateNew = row.PayRate,
                        NprNew = row.Npr,
                        OhrNew = row.Ohr,
                        ChargeRateNew = newPay + newNpr + newOhr,
                    });
                    continue;
                }

                var payRateChanged = row.PayRate.HasValue && row.PayRate.Value != live.PayRate;
                var nprChanged = row.Npr.HasValue && row.Npr.Value != live.Npr;
                var ohrChanged = row.Ohr.HasValue && row.Ohr.Value != live.Ohr;

                var effPay = row.PayRate ?? live.PayRate ?? 0m;
                var effNpr = row.Npr ?? live.Npr ?? 0m;
                var effOhr = row.Ohr ?? live.Ohr ?? 0m;
                var srcPay = live.PayRate ?? 0m;
                var srcNpr = live.Npr ?? 0m;
                var srcOhr = live.Ohr ?? 0m;
                rows.Add(new BulkRatesStaffStagingRowDto
                {
                    // The worker independently recomputes this same per-field diff at apply
                    // time and skips no-change rows there — this Status only drives display.
                    Status = (payRateChanged || nprChanged || ohrChanged) ? "Updated" : "No Change",
                    PcGrade = row.PcGrade,
                    PayRate = live.PayRate,
                    PayRateNew = row.PayRate,
                    Npr = live.Npr,
                    NprNew = row.Npr,
                    Ohr = live.Ohr,
                    OhrNew = row.Ohr,
                    ChargeRate = srcPay + srcNpr + srcOhr,
                    ChargeRateNew = effPay + effNpr + effOhr,
                });
            }

            return new BulkRatesStagingDataDto { StaffRows = rows.OrderBy(r => StaffAnimalSortKey(r.Status)).ToList() };
        }

        public async Task<byte[]> ExportStagingDataAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var stagedStaff = await _repository.GetProfitCentreGradeStagingRowsAsync(jobQueueId, ct);

            _logger.LogInformation(
                "[BulkRates.ExportStagingData] JobQueueId={JobQueueId} | StaffRows={StaffRows}",
                jobQueueId, stagedStaff.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildStaffSheet(stagedStaff));
        }

        // ── Context building ─────────────────────────────────────────────────────────

        private async Task<StaffValidationContext> BuildContextAsync(
            Guid jobQueueId, int fpsYear, IReadOnlyList<ProfitCentreGradeStagingRow> stagedRows, CancellationToken ct)
        {
            var liveStaffRows = await _repository.GetStaffRowsForExportAsync(fpsYear, ct);
            var liveStaffLookup = liveStaffRows.ToDictionary(
                r => PcGrade(r.PcGrade),
                r => new LiveStaffRow { PcGrade = r.PcGrade, PayRate = r.PayRate, Npr = r.Npr, Ohr = r.Ohr });

            var stagedStaff = stagedRows.Select((r, i) => new ValidationStaffRow
            {
                PcGrade = r.PcGrade,
                PayRate = r.PayRate,
                Npr = r.Npr,
                Ohr = r.Ohr,
                SourceRow = i + 2
            }).ToList();

            return new StaffValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                LiveStaffLookup = liveStaffLookup,
                StagedStaffRows = stagedStaff
            };
        }

        // ── Validation rules (moved from StaffAnimalValidationService's Staff half) ──────

        /// <summary>
        /// Invalid-data checks (missing/duplicate key, negative rate) take priority over
        /// NotFound — they're a property of the uploaded row itself, independent of whether a
        /// live counterpart exists — so a row failing both is reported as Invalid, not NotFound.
        /// </summary>
        private static List<StaffValidationResult> ValidateStaff(StaffValidationContext ctx)
        {
            var results = new List<StaffValidationResult>();

            var duplicates = ctx.StagedStaffRows
                .Where(r => !string.IsNullOrWhiteSpace(r.PcGrade))
                .GroupBy(r => PcGrade(r.PcGrade))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedStaffRows)
            {
                if (string.IsNullOrWhiteSpace(row.PcGrade))
                {
                    results.Add(new StaffValidationResult
                    {
                        PcGrade = row.PcGrade,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("MISSING_GRADE", row.SourceRow, "PcGrade is required.")],
                    });
                    continue;
                }

                var key = PcGrade(row.PcGrade);
                var errors = new List<ValidationFinding>();

                if (duplicates.Contains(key))
                    errors.Add(Error("DUPLICATE_GRADE", row.SourceRow, $"Grade '{row.PcGrade}' appears more than once.", row.PcGrade));
                if (row.PayRate is < 0)
                    errors.Add(Error("NEGATIVE_RATE", row.SourceRow, "Negative rates are not permitted.", row.PcGrade, "payrate"));
                if (row.Npr is < 0)
                    errors.Add(Error("NEGATIVE_RATE", row.SourceRow, "Negative rates are not permitted.", row.PcGrade, "npr"));
                if (row.Ohr is < 0)
                    errors.Add(Error("NEGATIVE_RATE", row.SourceRow, "Negative rates are not permitted.", row.PcGrade, "ohr"));

                var hasLive = ctx.LiveStaffLookup.TryGetValue(key, out var live);
                var effectivePayRate = StaffAnimalFieldComparer.NormalizeAmount(row.PayRate);
                var effectiveNpr = StaffAnimalFieldComparer.NormalizeAmount(row.Npr);
                var effectiveOhr = StaffAnimalFieldComparer.NormalizeAmount(row.Ohr);
                var effective = new StaffFieldState
                {
                    PayRate = effectivePayRate,
                    Npr = effectiveNpr,
                    Ohr = effectiveOhr,
                    ChargeRate = effectivePayRate + effectiveNpr + effectiveOhr,
                };

                if (errors.Count > 0)
                {
                    results.Add(new StaffValidationResult
                    {
                        PcGrade = row.PcGrade,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        Source = hasLive ? ToState(live!) : null,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = errors,
                    });
                    continue;
                }

                if (!hasLive)
                {
                    results.Add(new StaffValidationResult
                    {
                        PcGrade = row.PcGrade,
                        Action = StaffAnimalCalculatedAction.NotFound,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("GRADE_NOT_FOUND", row.SourceRow, $"PcGrade '{row.PcGrade}' does not exist.", row.PcGrade)],
                    });
                    continue;
                }

                var unchanged =
                    StaffAnimalFieldComparer.AmountEquals(row.PayRate, live!.PayRate) &&
                    StaffAnimalFieldComparer.AmountEquals(row.Npr, live.Npr) &&
                    StaffAnimalFieldComparer.AmountEquals(row.Ohr, live.Ohr);

                results.Add(new StaffValidationResult
                {
                    PcGrade = row.PcGrade,
                    Action = unchanged ? StaffAnimalCalculatedAction.NoChange : StaffAnimalCalculatedAction.Update,
                    Source = ToState(live),
                    Effective = effective,
                    ValidationVersion = StaffAnimalValidationVersion.Current,
                });
            }

            return results;
        }

        private static StaffFieldState ToState(LiveStaffRow live)
        {
            var payRate = StaffAnimalFieldComparer.NormalizeAmount(live.PayRate);
            var npr = StaffAnimalFieldComparer.NormalizeAmount(live.Npr);
            var ohr = StaffAnimalFieldComparer.NormalizeAmount(live.Ohr);
            return new StaffFieldState { PayRate = payRate, Npr = npr, Ohr = ohr, ChargeRate = payRate + npr + ohr };
        }

        private static ValidationFinding Error(string code, int sourceRow, string message, string? businessKey = null, string? field = null)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Error,
                Sheet = "Staff",
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Field = field,
                Message = message,
            };

        // ── Business-key normalization ───────────────────────────────────────────────

        /// <summary>
        /// Staff-only half of the old combined <c>StaffAnimalValidationKeys</c> — inlined here
        /// per Phase 4 rather than continuing to share one file whose other half
        /// (<c>AnimalType</c>) only <c>BulkAnimalRatesService</c> uses.
        /// </summary>
        private static string PcGrade(string pcGrade) => pcGrade.ToUpperInvariant();

        // ── Staging-grid presentation helpers ─────────────────────────────────────────

        // Sort order for Staff/Animal: Not Found first, Updated next, No Change last.
        private static int StaffAnimalSortKey(string status) => status switch
        {
            "Not Found" => 0,
            "Updated" => 1,
            "No Change" => 2,
            _ => 0
        };

        // Sheet name matches BulkRatesExcelParser's StaffSheet ("Staff") so a downloaded template
        // re-uploads without modification.
        private static List<ExcelSheetDefinition> BuildStaffSheet(IReadOnlyList<ProfitCentreGradeStagingRow> rows)
        {
            var exportRows = rows.Select(r => new BulkRatesStaffExportRowDto
            {
                PcGrade = r.PcGrade,
                PayRate = r.PayRate,
                Npr = r.Npr,
                Ohr = r.Ohr
            }).ToList();

            // PcGrade is the sole identity/business key — protect it so a retyped grade can't
            // silently produce an unmatched "Not Found" row on re-upload, matching FEC/AGRUP's
            // template protection. PayRate/Npr/Ohr stay editable. Staff is update-only, so there's
            // no insert-a-new-row path this protection could block.
            return [new()
            {
                SheetName = "Staff",
                Data = exportRows.Cast<object>(),
                DataType = typeof(BulkRatesStaffExportRowDto)
            }];
        }

        // ── Temporary private duplicates of the still-standalone Staff-only support types
        // under Validation/BulkRates/ (see class-level doc comment). ────────────────────

        private sealed record StaffValidationContext
        {
            public required Guid JobQueueId { get; init; }
            public required int FpsYear { get; init; }
            public required IReadOnlyDictionary<string, LiveStaffRow> LiveStaffLookup { get; init; }
            public required IReadOnlyList<ValidationStaffRow> StagedStaffRows { get; init; }
        }

        private sealed record LiveStaffRow
        {
            public required string PcGrade { get; init; }
            public decimal? PayRate { get; init; }
            public decimal? Npr { get; init; }
            public decimal? Ohr { get; init; }
        }

        private sealed record ValidationStaffRow
        {
            public required string PcGrade { get; init; }
            public decimal? PayRate { get; init; }
            public decimal? Npr { get; init; }
            public decimal? Ohr { get; init; }
            public required int SourceRow { get; init; }
        }

        private sealed record StaffFieldState
        {
            public required decimal PayRate { get; init; }
            public required decimal Npr { get; init; }
            public required decimal Ohr { get; init; }
            public required decimal ChargeRate { get; init; }
        }

        private sealed record StaffValidationResult
        {
            public required string PcGrade { get; init; }
            public required string Action { get; init; }
            public StaffFieldState? Source { get; init; }
            public StaffFieldState? Effective { get; init; }
            public required int ValidationVersion { get; init; }
            public IReadOnlyList<ValidationFinding> Errors { get; init; } = [];
        }
    }
}
