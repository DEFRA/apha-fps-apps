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
    /// Owns the FEC/AGRUP ("Test") Bulk Rates process end to end — upload validation and
    /// staging, release-time revalidation and freeze, and staging/export/download
    /// presentation. Extracted from <c>BulkRatesValidator</c>/<c>BulkRatesValidationService</c>/
    /// <c>BulkRatesRequestService</c> per the low-risk phase-wise execution plan, Phase 2.
    /// Not yet wired into <c>BulkRatesRequestService</c> or DI — the original classes remain
    /// fully operational until this is proven and Phase 5+ switch callers over.
    ///
    /// The private nested <c>ValidationContext</c>/<c>ValidationFecRow</c>/<c>ValidationAgrupRow</c>/
    /// <c>LiveFecRow</c>/<c>LiveAgrupRow</c>/<c>DownloadedSnapshotKey</c>/<c>ValidationCalculatedAction</c>/
    /// <c>BulkRatesValidationKeys</c> types below are deliberate temporary duplicates of the
    /// still-standalone types under <c>Validation/BulkRates/</c> (confirmed FEC/AGRUP-only —
    /// Staff/Animal never reference them) — the originals stay in place because
    /// <c>BulkRatesValidator</c> still depends on them until Phase 8 deletes it. Only
    /// <see cref="ValidationFinding"/>/<see cref="ValidationSeverity"/> are referenced directly
    /// (genuinely shared with Staff/Animal too, so not duplicated here).
    /// </summary>
    public class BulkTestRatesService : IBulkTestRatesService
    {
        private readonly IBulkRatesRepository _repository;
        private readonly IExcelExportService _excelExportService;
        private readonly ILogger<BulkTestRatesService> _logger;

        public BulkTestRatesService(
            IBulkRatesRepository repository,
            IExcelExportService excelExportService,
            ILogger<BulkTestRatesService> logger)
        {
            _repository = repository;
            _excelExportService = excelExportService;
            _logger = logger;
        }

        // ── Upload ───────────────────────────────────────────────────────────────

        public async Task<BulkRatesValidationResult> ProcessUploadAsync(
            BulkRatesParseResult parseResult, int fpsYear, int uploadVersion, int? downloadVersion,
            CancellationToken ct = default)
        {
            // File-level parse errors become Error-severity validation entries on row 0.
            // Staging is still replaced with whatever rows did parse — this must not become
            // "parse errors -> return immediately", which would lose already-good rows
            // (§1c of the implementation plan).
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

                await _repository.ReplaceStagingFecAsync(
                    parseResult.JobQueueId, parseResult.FecRows, parseResult.AgrupRows, ct);

                return new BulkRatesValidationResult { Errors = fileErrors, RowCounts = new() };
            }

            var context = await BuildContextAsync(
                parseResult.JobQueueId, fpsYear, uploadVersion, downloadVersion,
                parseResult.FecRows, parseResult.AgrupRows, includeWorkerOnlyChecks: false, ct);
            var findings = Validate(context);

            // ROW_CLASSIFIED findings (Info severity) are the per-row calculated-action
            // output, not user-facing validation errors — they drive RowCounts below, not
            // fps.staging_validation_error.
            var errors = findings
                .Where(f => f.ValidationCode != "ROW_CLASSIFIED")
                .Select(f => BulkRatesValidationFindingMapper.MapFinding(f, parseResult.JobQueueId, uploadVersion))
                .ToList();

            var counts = ComputeRowCounts(parseResult.FecRows.Count, parseResult.AgrupRows.Count, findings, errors);

            await _repository.ReplaceStagingFecAsync(
                parseResult.JobQueueId, parseResult.FecRows, parseResult.AgrupRows, ct);

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        // ── Release-time re-validation + freeze ─────────────────────────────────────

        /// <summary>
        /// Re-runs the same rules against the currently staged rows (read back from the DB —
        /// release time has no fresh parseResult in hand) and current live/reference data, and
        /// freezes the reviewed classification onto staging once clean. Throws
        /// <see cref="BusinessValidationErrorException"/> on blocking errors, ProjectService-style,
        /// rather than returning a BlockingErrors list the caller has to remember to check.
        /// </summary>
        public async Task PrepareForReleaseAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion,
            CancellationToken ct = default)
        {
            var fecRows = await _repository.GetTestOrProductStagingRowsAsync(jobQueueId, ct);
            var agrupRows = await _repository.GetTestRequirementStagingRowsAsync(jobQueueId, ct);

            var context = await BuildContextAsync(
                jobQueueId, fpsYear, uploadVersion, downloadVersion, fecRows, agrupRows,
                includeWorkerOnlyChecks: false, ct);
            var findings = Validate(context);

            var blockingErrors = findings.Where(f => f.Severity == ValidationSeverity.Error).ToList();
            if (blockingErrors.Count > 0)
                throw new BusinessValidationErrorException(blockingErrors
                    .Select(f => new BusinessValidationError(f.Message, f.ValidationCode))
                    .ToList());

            var fecFreezes = findings
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED" && string.Equals(f.Sheet, "FEC", StringComparison.OrdinalIgnoreCase))
                .Select(f =>
                {
                    context.LiveFecLookup.TryGetValue(BulkRatesValidationKeys.TestCode(f.BusinessKey!), out var live);
                    return new TestFreezeEntry(f.BusinessKey!, null, f.CalculatedAction!, f.EffectiveNewRate, live?.DefraUnitPrice);
                })
                .ToList();

            var agrupFreezes = findings
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED" && string.Equals(f.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase))
                .Select(f =>
                {
                    var (testCode, buyer) = BulkRatesValidationFindingMapper.SplitBusinessKey(f.Sheet, f.BusinessKey);
                    context.LiveAgrupLookup.TryGetValue(BulkRatesValidationKeys.AgrupKey(testCode!, buyer!), out var live);
                    return new TestFreezeEntry(testCode!, buyer, f.CalculatedAction!, f.EffectiveNewRate, live?.UnitPrice);
                })
                .ToList();

            await _repository.FreezeStagingCalculatedActionsAsync(jobQueueId, uploadVersion, fecFreezes, agrupFreezes, ct);
        }

        // ── Export / download ───────────────────────────────────────────────────────

        public async Task<byte[]> ExportTestDataAsync(int fpsYear, CancellationToken ct = default)
        {
            var fecRows = await _repository.GetFecRowsForExportAsync(fpsYear, ct);
            var agrupRows = await _repository.GetAgrupRowsForExportAsync(fpsYear, ct);

            _logger.LogInformation(
                "[BulkRates.ExportFecTestData] FpsYear={FpsYear} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                fpsYear, fecRows.Count, agrupRows.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildFecAgrupSheets(fecRows, agrupRows));
        }

        public async Task<byte[]> DownloadTestDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
        {
            var downloadVersion = await _repository.GetNextDownloadVersionAsync(entry.JobQueueId, ct);

            // Steps 1-2: snapshot live data as the new Generating header, in one transaction —
            // this becomes the immutable record the worker's revalidation validates against,
            // regardless of whether workbook generation below succeeds.
            var liveFec = await _repository.GetFecRowsForExportAsync(entry.FpsYear, ct);
            var liveAgrup = await _repository.GetAgrupRowsForExportAsync(entry.FpsYear, ct);
            await _repository.CreateDownloadSnapshotAsync(entry.JobQueueId, downloadVersion, liveFec, liveAgrup, ct);

            try
            {
                // Step 3: generate from the just-persisted snapshot, not a second live query —
                // the snapshot and the workbook can never disagree because they share one source.
                var snapshotFec = await _repository.GetFecSnapshotRowsAsync(entry.JobQueueId, downloadVersion, ct);
                var snapshotAgrup = await _repository.GetAgrupSnapshotRowsAsync(entry.JobQueueId, downloadVersion, ct);

                var metadata = new Dictionary<string, string>
                {
                    [BulkRatesDownloadMetadataKeys.JobQueueId] = entry.JobQueueId.ToString(),
                    [BulkRatesDownloadMetadataKeys.DownloadVersion] = downloadVersion.ToString(),
                };
                var bytes = _excelExportService.ExportToExcelMultiSheet(
                    BuildFecAgrupSheets(snapshotFec, snapshotAgrup), metadata);

                // Step 4: only on success, Ready + active_download_version — if anything above
                // throws, the header is left Generating/marked Failed below and
                // active_download_version stays untouched, so the previous still-valid version
                // (if any) remains what an upload is checked against.
                await _repository.MarkDownloadReadyAsync(entry.JobQueueId, downloadVersion, ct);

                _logger.LogInformation(
                    "[BulkRates.DownloadFecTestData] JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                    entry.JobQueueId, downloadVersion, snapshotFec.Count, snapshotAgrup.Count);

                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[BulkRates.DownloadFecTestData] Workbook generation failed after snapshot commit | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion}",
                    entry.JobQueueId, downloadVersion);
                await _repository.MarkDownloadFailedAsync(entry.JobQueueId, downloadVersion, ct);
                throw;
            }
        }

        // ── Staging grid / export ────────────────────────────────────────────────────

        public async Task<BulkRatesStagingDataDto> GetStagingDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
        {
            var stagedFec = await _repository.GetTestOrProductStagingRowsAsync(entry.JobQueueId, ct);
            var stagedAgrup = await _repository.GetTestRequirementStagingRowsAsync(entry.JobQueueId, ct);

            // Once a request has Completed, its staging rows are purged as post-commit cleanup —
            // there is nothing left to diff against live data. Skipping the live fetch here isn't
            // just an optimisation: treating "no staged rows" as "every live row was deleted"
            // would be actively wrong, since the apply step only ever touches rows that were
            // actually staged.
            var liveFec = entry.Status == "Completed"
                ? []
                : await _repository.GetFecRowsForExportAsync(entry.FpsYear, ct);

            var stagedTestCodes = stagedFec.Select(r => r.TestCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // The per-row action label comes from the validator — frozen at release time once
            // available, or computed live via GetCalculatedActionsAsync before release — never a
            // bespoke UI-layer diff. "Deleted" below is a separate concept the validator has no
            // row to classify: a live TestCode/Buyer this upload never staged at all.
            var needsLiveClassification =
                stagedFec.Any(r => r.CalculatedAction is null) || stagedAgrup.Any(r => r.CalculatedAction is null);
            var liveClassifications = needsLiveClassification
                ? await GetCalculatedActionsAsync(
                    entry.JobQueueId, entry.FpsYear, entry.UploadVersion ?? 0, entry.ActiveDownloadVersion, ct)
                : [];

            var fecActionByTestCode = liveClassifications
                .Where(f => string.Equals(f.Sheet, "FEC", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f => f.BusinessKey!, StringComparer.OrdinalIgnoreCase);
            var agrupActionByKey = liveClassifications
                .Where(f => string.Equals(f.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f =>
                {
                    var parts = f.BusinessKey!.Split('/', 2);
                    return AgrupKey(parts[0], parts.Length > 1 ? parts[1] : string.Empty);
                });

            // Build error-keyed lookups so rows with validation errors show "Error", not "Unknown".
            var storedErrors = await _repository.GetValidationErrorsAsync(entry.JobQueueId, ct);
            var fecTestCodesWithErrors = storedErrors
                .Where(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase)
                         && !string.IsNullOrEmpty(e.TestCode)
                         && (e.SheetName is null || string.Equals(e.SheetName, "FEC", StringComparison.OrdinalIgnoreCase)))
                .Select(e => e.TestCode!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var agrupKeysWithErrors = storedErrors
                .Where(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase)
                         && !string.IsNullOrEmpty(e.TestCode)
                         && string.Equals(e.SheetName, "AGRUP", StringComparison.OrdinalIgnoreCase))
                .Select(e => AgrupKey(e.TestCode!, e.Buyer ?? string.Empty))
                .ToHashSet();

            var fecRows = new List<BulkRatesFecStagingRowDto>();
            foreach (var row in stagedFec)
            {
                var calculatedAction = row.CalculatedAction;
                if (calculatedAction is null && fecActionByTestCode.TryGetValue(row.TestCode, out var liveFinding))
                    calculatedAction = liveFinding.CalculatedAction;

                fecRows.Add(new BulkRatesFecStagingRowDto
                {
                    Status = fecTestCodesWithErrors.Contains(row.TestCode)
                        ? "Error"
                        : FormatCalculatedAction(calculatedAction),
                    TestCode = row.TestCode,
                    UnitPriceVla = row.UnitPriceVla,
                    DefraUnitPrice = row.DefraUnitPrice,
                    FecNewRate = row.FecNewRate,
                    ItemDescription = row.ItemDescription,
                    ShortDescription = row.ShortDescription,
                    Owner = row.Owner,
                    Comments = row.Comments
                });
            }

            foreach (var live in liveFec)
            {
                if (stagedTestCodes.Contains(live.TestCode))
                    continue;

                fecRows.Add(new BulkRatesFecStagingRowDto
                {
                    Status = "Deleted",
                    TestCode = live.TestCode,
                    UnitPriceVla = live.UnitPriceVla,
                    DefraUnitPrice = live.DefraUnitPrice,
                    ItemDescription = live.ItemDescription,
                    ShortDescription = live.ShortDescription,
                    Owner = live.Owner,
                    Comments = live.Comments
                });
            }

            // Same rationale as liveFec above — AGRUP staging is purged alongside FEC on commit.
            var liveAgrup = entry.Status == "Completed"
                ? []
                : await _repository.GetAgrupRowsForExportAsync(entry.FpsYear, ct);
            var stagedAgrupKeys = stagedAgrup.Select(r => AgrupKey(r.TestCode, r.Buyer)).ToHashSet();

            var agrupRows = new List<BulkRatesAgrupStagingRowDto>();
            foreach (var row in stagedAgrup)
            {
                var calculatedAction = row.CalculatedAction;
                if (calculatedAction is null && agrupActionByKey.TryGetValue(AgrupKey(row.TestCode, row.Buyer), out var liveFinding))
                    calculatedAction = liveFinding.CalculatedAction;

                agrupRows.Add(new BulkRatesAgrupStagingRowDto
                {
                    Status = agrupKeysWithErrors.Contains(AgrupKey(row.TestCode, row.Buyer))
                        ? "Error"
                        : FormatCalculatedAction(calculatedAction),
                    TestCode = row.TestCode,
                    Buyer = row.Buyer,
                    Agrup = row.Agrup,
                    AgrupNew = row.AgrupNew,
                    NoRequired = row.NoRequired,
                    DateCreated = row.DateCreated,
                    Active = row.Active,
                    Comments = row.Comments
                });
            }

            foreach (var live in liveAgrup)
            {
                if (stagedAgrupKeys.Contains(AgrupKey(live.TestCode, live.Buyer)))
                    continue;

                agrupRows.Add(new BulkRatesAgrupStagingRowDto
                {
                    Status = "Deleted",
                    TestCode = live.TestCode,
                    Buyer = live.Buyer,
                    Agrup = live.Agrup,
                    NoRequired = live.NoRequired,
                    DateCreated = live.DateCreated,
                    Active = live.Active,
                    Comments = live.Comments
                });
            }

            return new BulkRatesStagingDataDto
            {
                FecRows = fecRows.OrderBy(r => FecAgrupSortKey(r.Status)).ToList(),
                AgrupRows = agrupRows.OrderBy(r => FecAgrupSortKey(r.Status)).ToList()
            };
        }

        public async Task<byte[]> ExportStagingDataAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var stagedFec = await _repository.GetTestOrProductStagingRowsAsync(jobQueueId, ct);
            var stagedAgrup = await _repository.GetTestRequirementStagingRowsAsync(jobQueueId, ct);

            _logger.LogInformation(
                "[BulkRates.ExportStagingData] JobQueueId={JobQueueId} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, stagedFec.Count, stagedAgrup.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildFecAgrupSheets(stagedFec, stagedAgrup));
        }

        // ── Calculated action for display, when nothing is frozen yet ────────────────

        private async Task<IReadOnlyList<ValidationFinding>> GetCalculatedActionsAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion, CancellationToken ct)
        {
            var fecRows = await _repository.GetTestOrProductStagingRowsAsync(jobQueueId, ct);
            var agrupRows = await _repository.GetTestRequirementStagingRowsAsync(jobQueueId, ct);

            var context = await BuildContextAsync(
                jobQueueId, fpsYear, uploadVersion, downloadVersion, fecRows, agrupRows,
                includeWorkerOnlyChecks: false, ct);

            return Validate(context)
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED")
                .ToList();
        }

        // ── Context building ─────────────────────────────────────────────────────────

        private async Task<ValidationContext> BuildContextAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion,
            IReadOnlyList<TestOrProductStagingRow> fecRows, IReadOnlyList<TestRequirementStagingRow> agrupRows,
            bool includeWorkerOnlyChecks, CancellationToken ct)
        {
            var liveFecRows = await _repository.GetFecRowsForExportAsync(fpsYear, ct);
            var liveAgrupRows = await _repository.GetAgrupRowsForExportAsync(fpsYear, ct);

            var liveFecLookup = liveFecRows.ToDictionary(
                r => BulkRatesValidationKeys.TestCode(r.TestCode),
                r => new LiveFecRow { TestCode = r.TestCode, UnitPriceVla = r.UnitPriceVla, DefraUnitPrice = r.DefraUnitPrice });

            var liveAgrupLookup = liveAgrupRows.ToDictionary(
                r => BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer),
                r => new LiveAgrupRow
                {
                    TestCode = r.TestCode,
                    Buyer = r.Buyer,
                    UnitPrice = r.Agrup,
                    ProjectBuyerCode = r.ProjectBuyerCode,
                    TestBuyerCode = r.TestBuyerCode
                });

            // Project/capability lookups are bulk but scoped to only the routing values this
            // upload actually supplies — not the entire reference table.
            var projectCodes = agrupRows
                .Where(r => !string.IsNullOrWhiteSpace(r.ProjectBuyerCode))
                .Select(r => r.ProjectBuyerCode!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var projectLookup = await _repository.GetExistingProjectCodesAsync(projectCodes, fpsYear, ct);

            var capabilityPairs = agrupRows
                .Where(r => !string.IsNullOrWhiteSpace(r.TestBuyerWorkGroup))
                .Select(r => (r.TestCode, r.TestBuyerWorkGroup!))
                .ToHashSet();
            var capabilityLookup = await _repository.GetExistingCapabilityPairsAsync(capabilityPairs, fpsYear, ct);

            IReadOnlyList<DownloadedSnapshotKey> frozenSnapshot = [];
            if (downloadVersion.HasValue)
            {
                var snapshotFec = await _repository.GetFecSnapshotRowsAsync(jobQueueId, downloadVersion.Value, ct);
                var snapshotAgrup = await _repository.GetAgrupSnapshotRowsAsync(jobQueueId, downloadVersion.Value, ct);
                frozenSnapshot = snapshotFec
                    .Select(r => new DownloadedSnapshotKey { Sheet = "FEC", TestCode = r.TestCode, SourceRate = r.DefraUnitPrice })
                    .Concat(snapshotAgrup.Select(r => new DownloadedSnapshotKey { Sheet = "AGRUP", TestCode = r.TestCode, Buyer = r.Buyer, SourceRate = r.Agrup }))
                    .ToList();
            }

            var stagedFec = fecRows.Select((r, i) => new ValidationFecRow
            {
                TestCode = r.TestCode,
                FecNewRate = r.FecNewRate,
                ItemDescription = r.ItemDescription,
                ShortDescription = r.ShortDescription,
                Owner = r.Owner,
                Comments = r.Comments,
                SourceRow = i + 2
            }).ToList();

            var stagedAgrup = agrupRows.Select((r, i) =>
            {
                liveAgrupLookup.TryGetValue(BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer), out var live);
                return new ValidationAgrupRow
                {
                    TestCode = r.TestCode,
                    Buyer = r.Buyer,
                    AgrupNew = r.AgrupNew,
                    // Existing rows: the workbook has no column yet to assert a routing value.
                    // Until then, an absent staged value must echo the live one rather than read
                    // as "blanked out" — otherwise every ordinary rate-only update on a row that
                    // already has routing data would falsely trip the immutability check below.
                    // New rows have no live value to echo, so they correctly stay null and fall
                    // through to MISSING_ROUTING_FIELD until a workbook can actually supply one.
                    ProjectBuyerCode = r.ProjectBuyerCode ?? live?.ProjectBuyerCode,
                    TestBuyerCode = r.TestBuyerCode ?? live?.TestBuyerCode,
                    TestBuyerWorkGroup = r.TestBuyerWorkGroup,
                    Comments = r.Comments,
                    SourceRow = i + 2
                };
            }).ToList();

            return new ValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                DownloadVersion = downloadVersion,
                UploadVersion = uploadVersion,
                LiveFecLookup = liveFecLookup,
                LiveAgrupLookup = liveAgrupLookup,
                ProjectLookup = projectLookup,
                CapabilityLookup = capabilityLookup,
                StagedFecRows = stagedFec,
                StagedAgrupRows = stagedAgrup,
                FrozenSnapshot = frozenSnapshot,
                IncludeWorkerOnlyChecks = includeWorkerOnlyChecks
            };
        }

        // ── Validation rules (moved from BulkRatesValidationService) ─────────────────

        private static List<ValidationFinding> Validate(ValidationContext context)
        {
            var findings = new List<ValidationFinding>();

            var withdrawnFecTestCodes = ValidateFec(context, findings);
            ValidateAgrup(context, findings, withdrawnFecTestCodes);
            if (context.IncludeWorkerOnlyChecks)
                ValidateLiveWithdrawalConflicts(context, findings, withdrawnFecTestCodes);
            ValidateSnapshotPreservation(context, findings);

            return findings;
        }

        /// <summary>Validates all staged FEC rows and returns the set of TestCodes classified as ZeroRateWithdrawal (needed by the AGRUP-side withdrawal-conflict checks).</summary>
        private static HashSet<string> ValidateFec(ValidationContext ctx, ICollection<ValidationFinding> findings)
        {
            var withdrawn = new HashSet<string>();

            var duplicates = ctx.StagedFecRows
                .GroupBy(r => BulkRatesValidationKeys.TestCode(r.TestCode))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedFecRows)
            {
                var key = BulkRatesValidationKeys.TestCode(row.TestCode);

                if (duplicates.Contains(key))
                    findings.Add(Error("DUPLICATE_TEST_CODE", "FEC", row.TestCode, row.SourceRow,
                        $"TestCode '{row.TestCode}' appears more than once in the FEC worksheet."));

                if (row.FecNewRate is < 0)
                    findings.Add(Error("NEGATIVE_RATE", "FEC", row.TestCode, row.SourceRow,
                        "Negative rates are not permitted.", "fecnewrate"));

                if (!ctx.LiveFecLookup.TryGetValue(key, out var live))
                {
                    // New Test Code.
                    if (row.FecNewRate is null)
                        findings.Add(Error("MISSING_FEC_NEW_RATE", "FEC", row.TestCode, row.SourceRow,
                            "FEC New rate is mandatory for a new Test Code.", "fecnewrate"));
                    if (string.IsNullOrWhiteSpace(row.ItemDescription))
                        findings.Add(Error("MISSING_FOR_INSERT", "FEC", row.TestCode, row.SourceRow,
                            "Item Description is required for new Test Code inserts.", "itemdescription"));
                    if (string.IsNullOrWhiteSpace(row.ShortDescription))
                        findings.Add(Error("MISSING_FOR_INSERT", "FEC", row.TestCode, row.SourceRow,
                            "Short Description is required for new Test Code inserts.", "shortdescription"));
                    if (string.IsNullOrWhiteSpace(row.Owner))
                        findings.Add(Error("MISSING_FOR_INSERT", "FEC", row.TestCode, row.SourceRow,
                            "Owner is required for new Test Code inserts.", "owner"));

                    if (row.FecNewRate is >= 0)
                        findings.Add(Classification("FEC", row.TestCode, row.SourceRow,
                            ValidationCalculatedAction.Insert, row.FecNewRate));
                }
                else
                {
                    // Existing Test Code: description/owner changes are ignored (spec, unchanged).

                    // Existing + blank/zero rate = Zero-Rate Withdrawal, not an error — still
                    // counts toward withdrawnFecTestCodes even when already zero (a positive
                    // AGRUP row under an already-zero FEC Test Code is still a conflict,
                    // regardless of whether this upload is what caused the transition) — but the
                    // classification itself is NoChange, not a fresh withdrawal, when the live
                    // rate was already 0 (nothing is actually changing, so nothing should be
                    // written/audited as an update).
                    if (row.FecNewRate is null or 0)
                    {
                        withdrawn.Add(key);
                        var alreadyZero = (live.UnitPriceVla ?? 0) == 0 && (live.DefraUnitPrice ?? 0) == 0;
                        findings.Add(Classification("FEC", row.TestCode, row.SourceRow,
                            alreadyZero ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.ZeroRateWithdrawal,
                            0m));
                    }
                    else if (row.FecNewRate is > 0)
                    {
                        var unchanged = row.FecNewRate == live.UnitPriceVla && row.FecNewRate == live.DefraUnitPrice;
                        findings.Add(Classification("FEC", row.TestCode, row.SourceRow,
                            unchanged ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.Update,
                            row.FecNewRate));
                    }
                    // Negative already reported above; no classification for an invalid value.
                }
            }

            return withdrawn;
        }

        private static void ValidateAgrup(
            ValidationContext ctx, ICollection<ValidationFinding> findings, IReadOnlySet<string> withdrawnFecTestCodes)
        {
            var fecTestCodesInUpload = ctx.StagedFecRows
                .Select(r => BulkRatesValidationKeys.TestCode(r.TestCode))
                .ToHashSet();

            var duplicates = ctx.StagedAgrupRows
                .GroupBy(r => BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedAgrupRows)
            {
                var businessKey = $"{row.TestCode}/{row.Buyer}";
                var testCodeKey = BulkRatesValidationKeys.TestCode(row.TestCode);
                var agrupKey = BulkRatesValidationKeys.AgrupKey(row.TestCode, row.Buyer);

                if (duplicates.Contains(agrupKey))
                    findings.Add(Error("DUPLICATE_AGRUP_KEY", "AGRUP", businessKey, row.SourceRow,
                        $"TestCode+Buyer combination '{businessKey}' appears more than once."));

                var testCodeExists = ctx.LiveFecLookup.ContainsKey(testCodeKey) || fecTestCodesInUpload.Contains(testCodeKey);
                if (!testCodeExists)
                    findings.Add(Error("TEST_CODE_NOT_FOUND", "AGRUP", businessKey, row.SourceRow,
                        $"TestCode '{row.TestCode}' does not exist in FPS year {ctx.FpsYear} and is not being inserted by this FEC upload.", "testcode"));

                if (row.AgrupNew is < 0)
                    findings.Add(Error("NEGATIVE_RATE", "AGRUP", businessKey, row.SourceRow,
                        "Negative rates are not permitted.", "agrupnew"));

                if (!ctx.LiveAgrupLookup.TryGetValue(agrupKey, out var live))
                    ValidateNewAgrupRow(ctx, row, businessKey, findings);
                else
                    ValidateExistingAgrupRow(row, live, businessKey, findings);

                // "Same as FEC" comment cross-check.
                if (string.Equals(row.Comments, "Same as FEC", StringComparison.OrdinalIgnoreCase))
                {
                    var fecMatch = ctx.StagedFecRows.FirstOrDefault(
                        f => BulkRatesValidationKeys.TestCode(f.TestCode) == testCodeKey);
                    if (fecMatch is not null && row.AgrupNew.HasValue && fecMatch.FecNewRate.HasValue
                        && row.AgrupNew.Value != fecMatch.FecNewRate.Value)
                    {
                        findings.Add(Error("SAME_AS_FEC_MISMATCH", "AGRUP", businessKey, row.SourceRow,
                            $"Comments is 'Same as FEC' but Agrup New ({row.AgrupNew}) does not equal FEC New ({fecMatch.FecNewRate}) for TestCode '{row.TestCode}'.", "agrupnew"));
                    }
                }

                // Interim BC-05 safety net, staged-vs-withdrawal (release-time,
                // snapshot-independent — this is about what THIS upload contains, not what
                // was previously downloaded). The snapshot-scoped/live-data counterpart is
                // ValidateLiveWithdrawalConflicts below.
                if (withdrawnFecTestCodes.Contains(testCodeKey) && row.AgrupNew is > 0)
                {
                    findings.Add(Error("AGRUP_POSITIVE_FOR_WITHDRAWN_FEC", "AGRUP", businessKey, row.SourceRow,
                        $"FEC TestCode '{row.TestCode}' is being withdrawn (zeroed) in this upload, but AGRUP row '{businessKey}' still has a positive rate ({row.AgrupNew}).", "agrupnew"));
                }
            }
        }

        private static void ValidateNewAgrupRow(
            ValidationContext ctx, ValidationAgrupRow row, string businessKey, ICollection<ValidationFinding> findings)
        {
            if (row.AgrupNew is null)
            {
                findings.Add(Error("MISSING_FOR_INSERT", "AGRUP", businessKey, row.SourceRow,
                    "Agrup New is required for new TestCode+Buyer inserts.", "agrupnew"));
            }
            else if (row.AgrupNew.Value == 0)
            {
                // BC-01 temporary rule: block a new AGRUP row at zero until business confirms
                // permanent behaviour.
                findings.Add(Error("NEW_AGRUP_ZERO_RATE_BLOCKED", "AGRUP", businessKey, row.SourceRow,
                    "New AGRUP rows with a zero rate are not currently permitted, pending business confirmation (BC-01).", "agrupnew"));
            }

            var hasProjectBuyerCode = !string.IsNullOrWhiteSpace(row.ProjectBuyerCode);
            var hasWorkGroup = !string.IsNullOrWhiteSpace(row.TestBuyerWorkGroup);

            if (!hasProjectBuyerCode && !hasWorkGroup)
            {
                findings.Add(Error("MISSING_ROUTING_FIELD", "AGRUP", businessKey, row.SourceRow,
                    "At least one of ProjectBuyerCode or TestBuyerWorkGroup must be supplied for a new AGRUP row (BC-02)."));
            }
            else
            {
                if (hasProjectBuyerCode && !ctx.ProjectLookup.Contains(BulkRatesValidationKeys.TestCode(row.ProjectBuyerCode!)))
                    findings.Add(Error("INVALID_PROJECT_BUYER_CODE", "AGRUP", businessKey, row.SourceRow,
                        $"ProjectBuyerCode '{row.ProjectBuyerCode}' does not exist for FPS year {ctx.FpsYear}.", "projectbuyercode"));

                if (hasWorkGroup && !ctx.CapabilityLookup.Contains(BulkRatesValidationKeys.CapabilityKey(row.TestCode, row.TestBuyerWorkGroup!)))
                    findings.Add(Error("INVALID_TEST_BUYER_WORKGROUP", "AGRUP", businessKey, row.SourceRow,
                        $"TestCode '{row.TestCode}' / WorkGroup '{row.TestBuyerWorkGroup}' is not a recognised capability for FPS year {ctx.FpsYear}.", "testbuyerworkgroup"));
            }

            if (row.AgrupNew is > 0)
                findings.Add(Classification("AGRUP", businessKey, row.SourceRow, ValidationCalculatedAction.Insert, row.AgrupNew));
        }

        private static void ValidateExistingAgrupRow(
            ValidationAgrupRow row, LiveAgrupRow live, string businessKey, ICollection<ValidationFinding> findings)
        {
            // Existing-key routing-field immutability, Bulk-Rates-scoped — not a system-wide
            // tlkptestreqmt rule; other writers (e.g. the PACT maintenance path) may still permit
            // controlled changes. Comparison matches the citext columns' own case-insensitivity
            // and adds no extra trimming — Excel-introduced whitespace on an otherwise-unedited
            // protected cell is a known, accepted risk, not silently "fixed" here without
            // business confirmation.
            if (RoutingFieldChanged(row.ProjectBuyerCode, live.ProjectBuyerCode))
                findings.Add(Error("ROUTING_FIELD_CHANGED", "AGRUP", businessKey, row.SourceRow,
                    $"ProjectBuyerCode cannot be changed for an existing AGRUP row (was '{live.ProjectBuyerCode}').", "projectbuyercode"));

            if (RoutingFieldChanged(row.TestBuyerCode, live.TestBuyerCode))
                findings.Add(Error("ROUTING_FIELD_CHANGED", "AGRUP", businessKey, row.SourceRow,
                    $"TestBuyerCode cannot be changed for an existing AGRUP row (was '{live.TestBuyerCode}').", "testbuyercode"));

            // Existing + blank/zero rate = Zero-Rate Withdrawal, not a silent no-op. As with FEC
            // above: if the live rate is already 0, nothing is actually changing — classify
            // NoChange, not a fresh withdrawal.
            if (row.AgrupNew is null or 0)
            {
                var alreadyZero = (live.UnitPrice ?? 0) == 0;
                findings.Add(Classification("AGRUP", businessKey, row.SourceRow,
                    alreadyZero ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.ZeroRateWithdrawal,
                    0m));
            }
            else if (row.AgrupNew is > 0)
            {
                var unchanged = row.AgrupNew == live.UnitPrice;
                findings.Add(Classification("AGRUP", businessKey, row.SourceRow,
                    unchanged ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.Update,
                    row.AgrupNew));
            }
        }

        private static bool RoutingFieldChanged(string? staged, string? live)
            => !string.Equals(staged ?? string.Empty, live ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Only called when ValidationContext.IncludeWorkerOnlyChecks is true — see that
        /// property for why this must not run at API release time. A live positive AGRUP row
        /// related to a withdrawn FEC TestCode that was NOT present in the frozen download
        /// snapshot cannot have been caught by the staged-row check above — it didn't exist, or
        /// wasn't yet linked to that TestCode, at download time.
        /// </summary>
        private static void ValidateLiveWithdrawalConflicts(
            ValidationContext ctx, ICollection<ValidationFinding> findings, IReadOnlySet<string> withdrawnFecTestCodes)
        {
            if (withdrawnFecTestCodes.Count == 0)
                return;

            var snapshotAgrupKeys = ctx.FrozenSnapshot
                .Where(s => string.Equals(s.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase) && s.Buyer is not null)
                .Select(s => BulkRatesValidationKeys.AgrupKey(s.TestCode, s.Buyer!))
                .ToHashSet();

            foreach (var live in ctx.LiveAgrupLookup.Values)
            {
                var testCodeKey = BulkRatesValidationKeys.TestCode(live.TestCode);
                if (!withdrawnFecTestCodes.Contains(testCodeKey))
                    continue;
                if (live.UnitPrice is not > 0)
                    continue;

                var agrupKey = BulkRatesValidationKeys.AgrupKey(live.TestCode, live.Buyer);
                if (snapshotAgrupKeys.Contains(agrupKey))
                    continue; // already covered by the staged-row check in ValidateAgrup

                var businessKey = $"{live.TestCode}/{live.Buyer}";
                findings.Add(RequestLevel("LIVE_AGRUP_POSITIVE_FOR_WITHDRAWN_FEC", "AGRUP", businessKey,
                    $"FEC TestCode '{live.TestCode}' is being withdrawn, but live AGRUP row '{businessKey}' " +
                    "(not present at download time) still has a positive rate — BC-05 interim rule."));
            }
        }

        private static void ValidateSnapshotPreservation(ValidationContext ctx, ICollection<ValidationFinding> findings)
        {
            if (ctx.DownloadVersion is null)
                return; // nothing was ever downloaded for this request to compare against

            var stagedFecKeys = ctx.StagedFecRows
                .Select(r => BulkRatesValidationKeys.TestCode(r.TestCode))
                .ToHashSet();
            var stagedAgrupKeys = ctx.StagedAgrupRows
                .Select(r => BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer))
                .ToHashSet();

            foreach (var snap in ctx.FrozenSnapshot)
            {
                if (string.Equals(snap.Sheet, "FEC", StringComparison.OrdinalIgnoreCase))
                {
                    if (!stagedFecKeys.Contains(BulkRatesValidationKeys.TestCode(snap.TestCode)))
                        findings.Add(RequestLevel("MISSING_DOWNLOADED_KEY", "FEC", snap.TestCode,
                            $"FEC — Downloaded Test Code {snap.TestCode} is missing from the uploaded workbook."));
                }
                else if (string.Equals(snap.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase) && snap.Buyer is not null)
                {
                    var key = BulkRatesValidationKeys.AgrupKey(snap.TestCode, snap.Buyer);
                    if (!stagedAgrupKeys.Contains(key))
                        findings.Add(RequestLevel("MISSING_DOWNLOADED_KEY", "AGRUP", $"{snap.TestCode}/{snap.Buyer}",
                            $"AGRUP — Downloaded Test Code {snap.TestCode} / Buyer {snap.Buyer} is missing from the uploaded workbook."));
                }
            }
        }

        // ── Finding factories ────────────────────────────────────────────────────────

        private static ValidationFinding Error(
            string code, string sheet, string businessKey, int? sourceRow, string message, string? field = null)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Error,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Field = field,
                Message = message,
            };

        private static ValidationFinding RequestLevel(string code, string sheet, string businessKey, string message)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Error,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = null,
                IsRequestLevel = true,
                Message = message,
            };

        private static ValidationFinding Classification(
            string sheet, string businessKey, int sourceRow, string action, decimal? effectiveNewRate)
            => new()
            {
                ValidationCode = "ROW_CLASSIFIED",
                Severity = ValidationSeverity.Info,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Message = $"Calculated action: {action}.",
                CalculatedAction = action,
                EffectiveNewRate = effectiveNewRate,
            };


        // ── Row counts ───────────────────────────────────────────────────────────────

        private static BulkRatesRowCounts ComputeRowCounts(
            int totalFec, int totalAgrup, IReadOnlyList<ValidationFinding> findings, IReadOnlyList<StagingValidationError> errors)
        {
            int insert = 0, update = 0, unchanged = 0;
            int fecInsert = 0, fecUpdate = 0, fecUnchanged = 0;
            int agrupInsert = 0, agrupUpdate = 0, agrupUnchanged = 0;
            foreach (var f in findings)
            {
                if (f.ValidationCode != "ROW_CLASSIFIED") continue;
                bool isFec = string.Equals(f.Sheet, "FEC", StringComparison.OrdinalIgnoreCase);
                switch (f.CalculatedAction)
                {
                    case ValidationCalculatedAction.Insert:
                        insert++;
                        if (isFec) fecInsert++; else agrupInsert++;
                        break;
                    case ValidationCalculatedAction.Update:
                    case ValidationCalculatedAction.ZeroRateWithdrawal:
                        update++;
                        if (isFec) fecUpdate++; else agrupUpdate++;
                        break;
                    case ValidationCalculatedAction.NoChange:
                        unchanged++;
                        if (isFec) fecUnchanged++; else agrupUnchanged++;
                        break;
                }
            }

            var total = totalFec + totalAgrup;
            var invalid = errors.Count(e => e.Severity == ValidationSeverity.Error);
            var fecInvalid = errors.Count(e => e.Severity == ValidationSeverity.Error
                && string.Equals(e.SheetName, "FEC", StringComparison.OrdinalIgnoreCase));
            var agrupInvalid = errors.Count(e => e.Severity == ValidationSeverity.Error
                && string.Equals(e.SheetName, "AGRUP", StringComparison.OrdinalIgnoreCase));
            return new BulkRatesRowCounts
            {
                Total = total,
                Insert = insert,
                Update = update,
                Unchanged = unchanged,
                Invalid = invalid,
                Valid = total - invalid,
                FecTotal = totalFec,
                FecInsert = fecInsert,
                FecUpdate = fecUpdate,
                FecUnchanged = fecUnchanged,
                FecInvalid = fecInvalid,
                AgrupTotal = totalAgrup,
                AgrupInsert = agrupInsert,
                AgrupUpdate = agrupUpdate,
                AgrupUnchanged = agrupUnchanged,
                AgrupInvalid = agrupInvalid,
            };
        }

        // ── Staging-grid presentation helpers ─────────────────────────────────────────

        private static (string TestCode, string Buyer) AgrupKey(string testCode, string buyer) =>
            (testCode.ToUpperInvariant(), buyer.ToUpperInvariant());

        private static string FormatCalculatedAction(string? calculatedAction) => calculatedAction switch
        {
            ValidationCalculatedAction.NoChange => "No Change",
            ValidationCalculatedAction.Insert => "Insert",
            ValidationCalculatedAction.Update => "Update",
            ValidationCalculatedAction.ZeroRateWithdrawal => "Zero-Rate Withdrawal",
            _ => "Unknown"
        };

        // Sort order for FEC/AGRUP: Error/Unknown first, then actionable rows, NoChange last.
        private static int FecAgrupSortKey(string status) => status switch
        {
            "Error" => 0,
            "Unknown" => 0,
            "Insert" => 1,
            "Update" => 2,
            "Zero-Rate Withdrawal" => 3,
            "Deleted" => 4,
            "No Change" => 5,
            _ => 0
        };

        private static List<ExcelSheetDefinition> BuildFecAgrupSheets(
            IReadOnlyList<TestOrProductStagingRow> fecRows, IReadOnlyList<TestRequirementStagingRow> agrupRows)
        {
            var fecExportRows = fecRows.Select(r => new BulkRatesFecExportRowDto
            {
                TestCode = r.TestCode,
                UnitPriceVla = r.UnitPriceVla,
                DefraUnitPrice = r.DefraUnitPrice,
                FecNew = r.FecNewRate,
                Change = r.Change,
                ItemDescription = r.ItemDescription,
                ShortDescription = r.ShortDescription,
                Owner = r.Owner,
                Comments = r.Comments
            }).ToList();

            var agrupExportRows = agrupRows.Select(r => new BulkRatesAgrupExportRowDto
            {
                TestCode = r.TestCode,
                Buyer = r.Buyer,
                Agrup = r.Agrup,
                AgrupNew = r.AgrupNew,
                Change = r.Change,
                NoRequired = r.NoRequired,
                DateCreated = r.DateCreated,
                Active = r.Active,
                Comments = r.Comments,
                ProjectBuyerCode = r.ProjectBuyerCode,
                TestBuyerCode = r.TestBuyerCode,
                TestBuyerWorkGroup = r.TestBuyerWorkGroup
            }).ToList();

            // Change is a live Excel formula, not the stored value above — for the user's
            // visibility only; the backend ignores it and recalculates independently. Blank FEC
            // New/Agrup New maps to "0 minus current rate" rather than a blank result, matching
            // the existing-row blank/zero = Zero-Rate Withdrawal business rule instead of implying
            // "no change" for a row the reviewer deliberately cleared.
            return
            [
                BuildFecAgrupInstructionsSheet(),
                new()
                {
                    SheetName = "FEC",
                    Data = fecExportRows.Cast<object>(),
                    DataType = typeof(BulkRatesFecExportRowDto),
                    FormulaColumns = new Dictionary<string, string>
                    {
                        [nameof(BulkRatesFecExportRowDto.Change)] = "IF({FecNew}=\"\",0-{DefraUnitPrice},{FecNew}-{DefraUnitPrice})"
                    }
                },
                new()
                {
                    SheetName = "AGRUP",
                    Data = agrupExportRows.Cast<object>(),
                    DataType = typeof(BulkRatesAgrupExportRowDto),
                    FormulaColumns = new Dictionary<string, string>
                    {
                        [nameof(BulkRatesAgrupExportRowDto.Change)] = "IF({AgrupNew}=\"\",0-{Agrup},{AgrupNew}-{Agrup})"
                    }
                }
            ];
        }

        // Column references below are kept in lockstep with BulkRatesFecExportRowDto/
        // BulkRatesAgrupExportRowDto's actual property order (= actual Excel column letters)
        // rather than copied from the legacy process this replaces — the two have drifted
        // (e.g. AGRUP's routing columns J/K/L did not exist in the legacy workbook).
        private static ExcelSheetDefinition BuildFecAgrupInstructionsSheet()
        {
            // This same builder feeds three downloads with different purposes — an open request's
            // editable workbook (meant to be edited and re-uploaded), an ad-hoc year-level
            // reference dump, and a read-only staging-review export. The instructions below
            // describe how to fill in an editable copy; the preamble scopes that so a reference/
            // review copy of this same sheet doesn't read as "edit and upload this".
            var rows = new List<BulkRatesFecAgrupInstructionRowDto>
            {
                new() { Text = "These instructions apply if this workbook was downloaded from an open Bulk Rates request for editing and re-upload. If this copy was downloaded for reference or for staging review, it is read-only — do not edit or upload it." },
                new() { Item = "1", Text = "FEC Tab. You must either complete ALL rows in the worksheet OR delete those where there is no change." },
                new() { SubItem = "a", Text = "Where a Test Code is given a new FEC value, enter the value into Column D (FEC New) on the appropriate row in the worksheet." },
                new() { SubItem = "b", Text = "All rows must be completed. If the new FEC value is the same as the current rate, copy the value from Column C (Defra Unit Price) into Column D (FEC New). Leaving Column D blank is treated as a Zero-Rate Withdrawal, not \"no change\"." },
                new() { SubItem = "c", Text = "If a new Test Code is added, it can be added either at the end or by inserting a new row at the appropriate place in the worksheet." },
                new() { SubItem = "d", Text = "For a new Test Code record, the following columns must be completed:" },
                new() { ColumnRef = "A", Text = "Test Code" },
                new() { ColumnRef = "D", Text = "FEC New" },
                new() { ColumnRef = "F", Text = "Item Description" },
                new() { ColumnRef = "G", Text = "Short Description" },
                new() { ColumnRef = "H", Text = "Owner" },
                new() { SubItem = "e", Text = "The value in Column E (Change) is calculated automatically — do not enter a value directly." },
                new() { Item = "2", Text = "Agrup Tab. You must either complete ALL rows in the worksheet OR delete those where there is no change." },
                new() { SubItem = "a", Text = "Where the Agrup value is the same as the FEC value for the coming year, enter the value into Column D (Agrup New) and enter \"Same as FEC\" into Column I (Comments)." },
                new() { SubItem = "b", Text = "If the Agrup value is changed, but is not the same as the FEC value, enter the new value into Column D (Agrup New) and ensure Column I (Comments) is blank." },
                new() { SubItem = "c", Text = "If the Agrup value does not change, then do nothing. If, as a result, it is no longer the same as the FEC value, ensure Column I (Comments) is blank." },
                new() { SubItem = "d", Text = "If there is a new Agrup record to be added, it can either be appended or inserted into a new row at the appropriate place in the worksheet. Ensure that the following columns are completed:" },
                new() { ColumnRef = "A", Text = "Test Code" },
                new() { ColumnRef = "B", Text = "Buyer" },
                new() { ColumnRef = "D", Text = "Agrup New" },
                new() { ColumnRef = "I", Text = "Comments — should be \"Same as FEC\" or left empty." },
                new() { ColumnRef = "F, G, H", Text = "Can be completed if the information is known. If Column G (Date Created) is left empty, it will default to the date the request is uploaded." },
                new() { ColumnRef = "J and/or L", Text = "At least one of Project Buyer Code (J) or Test Buyer Work Group (L) must be completed for a new Agrup record, to establish routing. Test Buyer Code (K) is reference-only and does not need to be set." },
            };

            return new ExcelSheetDefinition
            {
                SheetName = "Instructions",
                Data = rows.Cast<object>(),
                DataType = typeof(BulkRatesFecAgrupInstructionRowDto)
            };
        }

        // ── Temporary private duplicates of the still-standalone FEC/AGRUP-only support
        // types under Validation/BulkRates/ (see class-level doc comment). ────────────────

        private sealed record ValidationContext
        {
            public required Guid JobQueueId { get; init; }
            public required int FpsYear { get; init; }
            public int? DownloadVersion { get; init; }
            public required int UploadVersion { get; init; }
            public required IReadOnlyDictionary<string, LiveFecRow> LiveFecLookup { get; init; }
            public required IReadOnlyDictionary<(string TestCode, string Buyer), LiveAgrupRow> LiveAgrupLookup { get; init; }
            public required IReadOnlySet<string> ProjectLookup { get; init; }
            public required IReadOnlySet<(string TestCode, string WorkGroup)> CapabilityLookup { get; init; }
            public required IReadOnlyList<ValidationFecRow> StagedFecRows { get; init; }
            public required IReadOnlyList<ValidationAgrupRow> StagedAgrupRows { get; init; }
            public required IReadOnlyList<DownloadedSnapshotKey> FrozenSnapshot { get; init; }
            public bool IncludeWorkerOnlyChecks { get; init; }
        }

        private sealed record ValidationFecRow
        {
            public required string TestCode { get; init; }
            public decimal? FecNewRate { get; init; }
            public string? ItemDescription { get; init; }
            public string? ShortDescription { get; init; }
            public string? Owner { get; init; }
            public string? Comments { get; init; }
            public required int SourceRow { get; init; }
        }

        private sealed record ValidationAgrupRow
        {
            public required string TestCode { get; init; }
            public required string Buyer { get; init; }
            public decimal? AgrupNew { get; init; }
            public string? ProjectBuyerCode { get; init; }
            public string? TestBuyerCode { get; init; }
            public string? TestBuyerWorkGroup { get; init; }
            public string? Comments { get; init; }
            public required int SourceRow { get; init; }
        }

        private sealed record LiveFecRow
        {
            public required string TestCode { get; init; }
            public decimal? UnitPriceVla { get; init; }
            public decimal? DefraUnitPrice { get; init; }
        }

        private sealed record LiveAgrupRow
        {
            public required string TestCode { get; init; }
            public required string Buyer { get; init; }
            public decimal? UnitPrice { get; init; }
            public string? ProjectBuyerCode { get; init; }
            public string? TestBuyerCode { get; init; }
            public double? NoRequired { get; init; }
            public short? Active { get; init; }
        }

        private sealed record DownloadedSnapshotKey
        {
            public required string Sheet { get; init; }
            public required string TestCode { get; init; }
            public string? Buyer { get; init; }
            public decimal? SourceRate { get; init; }
        }

        private static class ValidationCalculatedAction
        {
            public const string NoChange = "NoChange";
            public const string Insert = "Insert";
            public const string Update = "Update";
            public const string ZeroRateWithdrawal = "ZeroRateWithdrawal";
        }

        private static class BulkRatesValidationKeys
        {
            public static string TestCode(string testCode) => testCode.ToUpperInvariant();

            public static (string TestCode, string Buyer) AgrupKey(string testCode, string buyer)
                => (testCode.ToUpperInvariant(), buyer.ToUpperInvariant());

            public static (string TestCode, string WorkGroup) CapabilityKey(string testCode, string workGroup)
                => (testCode.ToUpperInvariant(), workGroup.ToUpperInvariant());
        }
    }
}
