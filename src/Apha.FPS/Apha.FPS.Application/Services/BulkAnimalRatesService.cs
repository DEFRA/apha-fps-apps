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
    /// Owns the Animal Bulk Rates process end to end — upload validation and staging,
    /// release-time revalidation and freeze, and staging/export/download presentation. Animal is
    /// update-only: no insert path, no ZeroRateWithdrawal. Extracted from
    /// <c>StaffAnimalValidationService</c>/<c>BulkRatesValidator</c>/<c>BulkRatesRequestService</c>
    /// per the low-risk phase-wise execution plan, Phase 4. Not yet wired into
    /// <c>BulkRatesRequestService</c> or DI.
    ///
    /// <c>LiveAnimalRow</c>/<c>ValidationAnimalRow</c>/<c>AnimalFieldState</c>/
    /// <c>AnimalValidationResult</c> below are private nested types — genuinely Animal-only
    /// (never shared with Staff). <see cref="StaffAnimalFieldComparer"/>/
    /// <see cref="StaffAnimalCalculatedAction"/>/<see cref="StaffAnimalValidationVersion"/> are
    /// referenced from <c>Common/BulkRates/</c> — genuinely shared with
    /// <see cref="BulkStaffRatesService"/>, now that both real consumers exist side by side. The
    /// old combined <c>StaffAnimalValidationKeys.AnimalType</c> is inlined below as a private
    /// Animal-only helper, mirroring <see cref="BulkStaffRatesService"/>'s own private
    /// <c>PcGrade</c> helper.
    /// </summary>
    public class BulkAnimalRatesService : IBulkAnimalRatesService
    {
        private readonly IBulkRatesRepository _repository;
        private readonly IExcelExportService _excelExportService;
        private readonly ILogger<BulkAnimalRatesService> _logger;

        public BulkAnimalRatesService(
            IBulkRatesRepository repository,
            IExcelExportService excelExportService,
            ILogger<BulkAnimalRatesService> logger)
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

                await _repository.ReplaceStagingAnimalAsync(parseResult.JobQueueId, parseResult.AnimalRows, ct);

                return new BulkRatesValidationResult { Errors = fileErrors, RowCounts = new() };
            }

            var context = await BuildContextAsync(parseResult.JobQueueId, fpsYear, parseResult.AnimalRows, ct);
            var results = ValidateAnimal(context);

            var errors = results
                .SelectMany(r => r.Errors)
                .Select(f => BulkRatesValidationFindingMapper.MapFinding(f, parseResult.JobQueueId, uploadVersion))
                .ToList();

            var counts = StaffAnimalRowCountHelper.ComputeRowCounts(parseResult.AnimalRows.Count, results.Select(r => r.Action), errors);

            await _repository.ReplaceStagingAnimalAsync(parseResult.JobQueueId, parseResult.AnimalRows, ct);

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        // ── Release-time re-validation + freeze ─────────────────────────────────────

        public async Task PrepareForReleaseAsync(Guid jobQueueId, int fpsYear, CancellationToken ct = default)
        {
            var stagedRows = await _repository.GetAnimalStagingRowsAsync(jobQueueId, ct);
            var context = await BuildContextAsync(jobQueueId, fpsYear, stagedRows, ct);
            var results = ValidateAnimal(context);

            var blockingErrors = results
                .SelectMany(r => r.Errors)
                .Where(f => f.Severity == ValidationSeverity.Error)
                .ToList();
            if (blockingErrors.Count > 0)
                throw new BusinessValidationErrorException(blockingErrors
                    .Select(f => new BusinessValidationError(f.Message, f.ValidationCode))
                    .ToList());

            var freezes = results.Select(r => new AnimalFreezeEntry(
                r.AnimalType, r.Action,
                r.Source?.DailyRate, r.Source?.DefraDailyRate, r.Source?.PlanByWeek, r.Source?.Species, r.Source?.SecurityLevel,
                r.Effective?.DailyRate, r.Effective?.DefraDailyRate, r.Effective?.PlanByWeek, r.Effective?.Species, r.Effective?.SecurityLevel)).ToList();

            await _repository.FreezeAnimalStagingAsync(jobQueueId, StaffAnimalValidationVersion.Current, freezes, ct);
        }

        // ── Export / download ───────────────────────────────────────────────────────

        public async Task<byte[]> ExportTestDataAsync(int fpsYear, CancellationToken ct = default)
        {
            var animalRows = await _repository.GetAnimalRowsForExportAsync(fpsYear, ct);

            _logger.LogInformation(
                "[BulkRates.ExportAnimalTestData] FpsYear={FpsYear} | AnimalRows={AnimalRows}",
                fpsYear, animalRows.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildAnimalSheet(animalRows));
        }

        public async Task<byte[]> DownloadTestDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
        {
            var downloadVersion = await _repository.GetNextDownloadVersionAsync(entry.JobQueueId, ct);

            var liveRows = await _repository.GetAnimalRowsForExportAsync(entry.FpsYear, ct);
            await _repository.CreateAnimalDownloadSnapshotAsync(entry.JobQueueId, downloadVersion, liveRows, ct);

            try
            {
                var snapshotRows = await _repository.GetAnimalSnapshotRowsAsync(entry.JobQueueId, downloadVersion, ct);

                var metadata = new Dictionary<string, string>
                {
                    [BulkRatesDownloadMetadataKeys.JobQueueId] = entry.JobQueueId.ToString(),
                    [BulkRatesDownloadMetadataKeys.DownloadVersion] = downloadVersion.ToString(),
                };
                var bytes = _excelExportService.ExportToExcelMultiSheet(BuildAnimalSheet(snapshotRows), metadata);

                await _repository.MarkDownloadReadyAsync(entry.JobQueueId, downloadVersion, ct);

                _logger.LogInformation(
                    "[BulkRates.DownloadAnimalTestData] JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | AnimalRows={AnimalRows}",
                    entry.JobQueueId, downloadVersion, snapshotRows.Count);

                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[BulkRates.DownloadAnimalTestData] Workbook generation failed after snapshot commit | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion}",
                    entry.JobQueueId, downloadVersion);
                await _repository.MarkDownloadFailedAsync(entry.JobQueueId, downloadVersion, ct);
                throw;
            }
        }

        // ── Staging grid / export ────────────────────────────────────────────────────

        // Staff/Animal are update-only: a staged row whose key doesn't exist live is skipped,
        // never inserted, and a live row absent from the upload is left untouched, never deleted.
        public async Task<BulkRatesStagingDataDto> GetStagingDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
        {
            var stagedAnimal = await _repository.GetAnimalStagingRowsAsync(entry.JobQueueId, ct);
            var liveAnimal = await _repository.GetAnimalRowsForExportAsync(entry.FpsYear, ct);
            var liveByType = liveAnimal.ToDictionary(r => r.AnimalType, StringComparer.OrdinalIgnoreCase);

            var rows = new List<BulkRatesAnimalStagingRowDto>();
            foreach (var row in stagedAnimal)
            {
                if (!liveByType.TryGetValue(row.AnimalType, out var live))
                {
                    rows.Add(new BulkRatesAnimalStagingRowDto
                    {
                        Status = "Not Found",
                        AnimalType = row.AnimalType,
                        Species = row.Species,
                        SecurityLevel = row.SecurityLevel,
                        DailyRateNew = row.DailyRate,
                        DefraDailyRateNew = row.DefraDailyRate,
                        PlanByWeek = row.PlanByWeek
                    });
                    continue;
                }

                var dailyRateChanged = row.DailyRate.HasValue && row.DailyRate.Value != live.DailyRate;
                var defraDailyRateChanged = row.DefraDailyRate.HasValue && row.DefraDailyRate.Value != live.DefraDailyRate;
                var planByWeekChanged = row.PlanByWeek.HasValue && row.PlanByWeek.Value != live.PlanByWeek;
                var speciesChanged = row.Species is not null && row.Species != live.Species;
                var securityLevelChanged = row.SecurityLevel is not null && row.SecurityLevel != live.SecurityLevel;
                var anyChanged = dailyRateChanged || defraDailyRateChanged || planByWeekChanged || speciesChanged || securityLevelChanged;

                rows.Add(new BulkRatesAnimalStagingRowDto
                {
                    // The worker independently recomputes this same per-field diff at apply
                    // time and skips no-change rows there — this Status only drives display.
                    Status = anyChanged ? "Updated" : "No Change",
                    AnimalType = row.AnimalType,
                    Species = row.Species ?? live.Species,
                    SecurityLevel = row.SecurityLevel ?? live.SecurityLevel,
                    DailyRate = live.DailyRate,
                    DailyRateNew = row.DailyRate,
                    DefraDailyRate = live.DefraDailyRate,
                    DefraDailyRateNew = row.DefraDailyRate,
                    PlanByWeek = row.PlanByWeek ?? live.PlanByWeek
                });
            }

            return new BulkRatesStagingDataDto { AnimalRows = rows.OrderBy(r => StaffAnimalSortKey(r.Status)).ToList() };
        }

        public async Task<byte[]> ExportStagingDataAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var stagedAnimal = await _repository.GetAnimalStagingRowsAsync(jobQueueId, ct);

            _logger.LogInformation(
                "[BulkRates.ExportStagingData] JobQueueId={JobQueueId} | AnimalRows={AnimalRows}",
                jobQueueId, stagedAnimal.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildAnimalSheet(stagedAnimal));
        }

        // ── Context building ─────────────────────────────────────────────────────────

        private async Task<AnimalValidationContext> BuildContextAsync(
            Guid jobQueueId, int fpsYear, IReadOnlyList<AnimalStagingRow> stagedRows, CancellationToken ct)
        {
            var liveAnimalRows = await _repository.GetAnimalRowsForExportAsync(fpsYear, ct);
            var liveAnimalLookup = liveAnimalRows.ToDictionary(
                r => AnimalType(r.AnimalType),
                r => new LiveAnimalRow
                {
                    AnimalType = r.AnimalType,
                    DailyRate = r.DailyRate,
                    DefraDailyRate = r.DefraDailyRate,
                    PlanByWeek = r.PlanByWeek ?? false,
                    Species = r.Species,
                    SecurityLevel = r.SecurityLevel
                });

            var stagedAnimal = stagedRows.Select((r, i) => new ValidationAnimalRow
            {
                AnimalType = r.AnimalType,
                DailyRate = r.DailyRate,
                DefraDailyRate = r.DefraDailyRate,
                PlanByWeek = r.PlanByWeek,
                Species = r.Species,
                SecurityLevel = r.SecurityLevel,
                SourceRow = i + 2
            }).ToList();

            return new AnimalValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                LiveAnimalLookup = liveAnimalLookup,
                StagedAnimalRows = stagedAnimal
            };
        }

        // ── Validation rules (moved from StaffAnimalValidationService's Animal half) ─────

        /// <summary>
        /// Invalid-data checks (missing/duplicate key, negative rate) take priority over
        /// NotFound — they're a property of the uploaded row itself, independent of whether a
        /// live counterpart exists — so a row failing both is reported as Invalid, not NotFound.
        /// </summary>
        private static List<AnimalValidationResult> ValidateAnimal(AnimalValidationContext ctx)
        {
            var results = new List<AnimalValidationResult>();

            var duplicates = ctx.StagedAnimalRows
                .Where(r => !string.IsNullOrWhiteSpace(r.AnimalType))
                .GroupBy(r => AnimalType(r.AnimalType))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedAnimalRows)
            {
                if (string.IsNullOrWhiteSpace(row.AnimalType))
                {
                    results.Add(new AnimalValidationResult
                    {
                        AnimalType = row.AnimalType,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("MISSING_ANIMAL_TYPE", row.SourceRow, "AnimalType is required.")],
                    });
                    continue;
                }

                var key = AnimalType(row.AnimalType);
                var errors = new List<ValidationFinding>();

                if (duplicates.Contains(key))
                    errors.Add(Error("DUPLICATE_ANIMAL_TYPE", row.SourceRow, $"AnimalType '{row.AnimalType}' appears more than once.", row.AnimalType));
                if (row.DailyRate is < 0)
                    errors.Add(Error("NEGATIVE_RATE", row.SourceRow, "Negative rates are not permitted.", row.AnimalType, "dailyrate"));
                if (row.DefraDailyRate is < 0)
                    errors.Add(Error("NEGATIVE_RATE", row.SourceRow, "Negative rates are not permitted.", row.AnimalType, "defradailyrate"));

                var hasLive = ctx.LiveAnimalLookup.TryGetValue(key, out var live);
                var effective = new AnimalFieldState
                {
                    DailyRate = StaffAnimalFieldComparer.NormalizeAmount(row.DailyRate),
                    DefraDailyRate = StaffAnimalFieldComparer.NormalizeAmount(row.DefraDailyRate),
                    PlanByWeek = StaffAnimalFieldComparer.NormalizeFlag(row.PlanByWeek),
                    Species = StaffAnimalFieldComparer.NormalizeText(row.Species),
                    SecurityLevel = StaffAnimalFieldComparer.NormalizeText(row.SecurityLevel),
                };

                if (errors.Count > 0)
                {
                    results.Add(new AnimalValidationResult
                    {
                        AnimalType = row.AnimalType,
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
                    results.Add(new AnimalValidationResult
                    {
                        AnimalType = row.AnimalType,
                        Action = StaffAnimalCalculatedAction.NotFound,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("ANIMAL_TYPE_NOT_FOUND", row.SourceRow, $"AnimalType '{row.AnimalType}' does not exist.", row.AnimalType)],
                    });
                    continue;
                }

                var unchanged =
                    StaffAnimalFieldComparer.AmountEquals(row.DailyRate, live!.DailyRate) &&
                    StaffAnimalFieldComparer.AmountEquals(row.DefraDailyRate, live.DefraDailyRate) &&
                    StaffAnimalFieldComparer.FlagEquals(row.PlanByWeek, live.PlanByWeek) &&
                    StaffAnimalFieldComparer.TextEquals(row.Species, live.Species) &&
                    StaffAnimalFieldComparer.TextEquals(row.SecurityLevel, live.SecurityLevel);

                results.Add(new AnimalValidationResult
                {
                    AnimalType = row.AnimalType,
                    Action = unchanged ? StaffAnimalCalculatedAction.NoChange : StaffAnimalCalculatedAction.Update,
                    Source = ToState(live),
                    Effective = effective,
                    ValidationVersion = StaffAnimalValidationVersion.Current,
                });
            }

            return results;
        }

        private static AnimalFieldState ToState(LiveAnimalRow live) => new()
        {
            DailyRate = StaffAnimalFieldComparer.NormalizeAmount(live.DailyRate),
            DefraDailyRate = StaffAnimalFieldComparer.NormalizeAmount(live.DefraDailyRate),
            PlanByWeek = live.PlanByWeek,
            Species = StaffAnimalFieldComparer.NormalizeText(live.Species),
            SecurityLevel = StaffAnimalFieldComparer.NormalizeText(live.SecurityLevel),
        };

        private static ValidationFinding Error(string code, int sourceRow, string message, string? businessKey = null, string? field = null)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Error,
                Sheet = "Animal",
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Field = field,
                Message = message,
            };

        // ── Business-key normalization ───────────────────────────────────────────────

        /// <summary>
        /// Animal-only half of the old combined <c>StaffAnimalValidationKeys</c> — inlined here
        /// per Phase 4, mirroring <see cref="BulkStaffRatesService"/>'s own private
        /// <c>PcGrade</c> helper.
        /// </summary>
        private static string AnimalType(string animalType) => animalType.ToUpperInvariant();

        // ── Staging-grid presentation helpers ─────────────────────────────────────────

        // Sort order for Staff/Animal: Not Found first, Updated next, No Change last.
        private static int StaffAnimalSortKey(string status) => status switch
        {
            "Not Found" => 0,
            "Updated" => 1,
            "No Change" => 2,
            _ => 0
        };

        // Sheet name matches BulkRatesExcelParser's AnimalSheet ("Animals") so a downloaded
        // template re-uploads without modification.
        private static List<ExcelSheetDefinition> BuildAnimalSheet(IReadOnlyList<AnimalStagingRow> rows)
        {
            var exportRows = rows.Select(r => new BulkRatesAnimalExportRowDto
            {
                AnimalType = r.AnimalType,
                Species = r.Species,
                SecurityLevel = r.SecurityLevel,
                DailyRate = r.DailyRate,
                DefraDailyRate = r.DefraDailyRate,
                PlanByWeek = r.PlanByWeek
            }).ToList();

            // AnimalType is the sole identity/business key — protect it only. Species/
            // SecurityLevel are NOT protected: this service actively applies changes to both
            // alongside the rate fields, so they're legitimately mutable business data here, not
            // immutable reference data. Animal is update-only, so there's no insert-a-new-row
            // path this protection could block.
            return [new()
            {
                SheetName = "Animals",
                Data = exportRows.Cast<object>(),
                DataType = typeof(BulkRatesAnimalExportRowDto)
            }];
        }

        // ── Temporary private duplicates of the still-standalone Animal-only support types
        // under Validation/BulkRates/ (see class-level doc comment). ────────────────────

        private sealed record AnimalValidationContext
        {
            public required Guid JobQueueId { get; init; }
            public required int FpsYear { get; init; }
            public required IReadOnlyDictionary<string, LiveAnimalRow> LiveAnimalLookup { get; init; }
            public required IReadOnlyList<ValidationAnimalRow> StagedAnimalRows { get; init; }
        }

        private sealed record LiveAnimalRow
        {
            public required string AnimalType { get; init; }
            public decimal? DailyRate { get; init; }
            public decimal? DefraDailyRate { get; init; }
            public bool PlanByWeek { get; init; }
            public string? Species { get; init; }
            public string? SecurityLevel { get; init; }
        }

        private sealed record ValidationAnimalRow
        {
            public required string AnimalType { get; init; }
            public decimal? DailyRate { get; init; }
            public decimal? DefraDailyRate { get; init; }
            public bool? PlanByWeek { get; init; }
            public string? Species { get; init; }
            public string? SecurityLevel { get; init; }
            public required int SourceRow { get; init; }
        }

        private sealed record AnimalFieldState
        {
            public required decimal DailyRate { get; init; }
            public required decimal DefraDailyRate { get; init; }
            public required bool PlanByWeek { get; init; }
            public string? Species { get; init; }
            public string? SecurityLevel { get; init; }
        }

        private sealed record AnimalValidationResult
        {
            public required string AnimalType { get; init; }
            public required string Action { get; init; }
            public AnimalFieldState? Source { get; init; }
            public AnimalFieldState? Effective { get; init; }
            public required int ValidationVersion { get; init; }
            public IReadOnlyList<ValidationFinding> Errors { get; init; } = [];
        }
    }
}
