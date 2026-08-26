using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Data access contract for all Bulk Rates API operations against fps schema tables.
    /// Implemented by BulkRatesRepository in Apha.FPS.DataAccess — the core job_queue
    /// lifecycle via EF/LINQ, staging/validation/download-snapshot tables via raw Npgsql
    /// (see the implementation's class-level doc comment for the split).
    /// </summary>
    public interface IBulkRatesRepository
    {
        // ── Job master / status lookup ───────────────────────────────────────────
        Task<int?> GetJobIdByNameAsync(string jobName, CancellationToken ct = default);
        Task<int?> GetStatusIdByNameAsync(int jobId, string statusName, CancellationToken ct = default);

        // ── Queue entry CRUD ─────────────────────────────────────────────────────
        Task<BulkRatesQueueRow> CreateRequestAsync(
            Guid jobQueueId, Guid jobExecutionId, int jobId, int initiatedStatusId,
            string requestedBy, DateTime requestedAtUtc, int fpsYear,
            CancellationToken ct = default);

        Task<BulkRatesQueueRow?> GetRequestAsync(Guid jobExecutionId, CancellationToken ct = default);

        /// <summary>
        /// Server-side paged/sorted list, matching the app-wide DataGrid pagination convention.
        /// <paramref name="sortBy"/> is validated against a column whitelist internally — never
        /// interpolated directly into SQL.
        /// </summary>
        Task<PagedData<BulkRatesQueueRow>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status,
            int page, int pageSize, string? sortBy, bool descending,
            CancellationToken ct = default);

        /// <summary>
        /// True when no request for <paramref name="jobName"/> is in a blocking status (Initiated,
        /// ReleasedForApproval, Approved, Running); false when one is. Used to enforce the
        /// single-active-request-per-job-type rule.
        /// </summary>
        Task<bool> CanInitiateRequestAsync(string jobName, CancellationToken ct = default);

        // ── Status transitions ───────────────────────────────────────────────────
        /// <summary>
        /// Performs a guarded UPDATE: only changes status if the current statusId matches
        /// <paramref name="expectedStatusId"/>. Returns true when the row was updated.
        /// </summary>
        Task<bool> TransitionStatusAsync(
            Guid jobQueueId, int expectedStatusId, int newStatusId,
            CancellationToken ct = default);

        Task SetApprovalAsync(
            Guid jobQueueId, Guid jobExecutionId,
            string approvedBy, DateTime approvedAtUtc,
            string triggeredBy, DateTime triggeredAtUtc,
            int approvedStatusId,
            CancellationToken ct = default);

        Task SetRejectionAsync(
            Guid jobQueueId, string rejectedBy, DateTime rejectedAtUtc,
            string reason, int rejectedStatusId,
            CancellationToken ct = default);

        Task SetCancellationAsync(
            Guid jobQueueId, string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default);

        // ── Upload metadata ──────────────────────────────────────────────────────
        Task UpdateUploadMetadataAsync(
            Guid jobQueueId, string filename, string checksumSha256, int uploadVersion,
            DateTime validatedAtUtc, string rowCountsJson, CancellationToken ct = default);

        Task UpdateS3ObjectKeyAsync(
            Guid jobQueueId, string s3ObjectKey, CancellationToken ct = default);

        // ── Audit log ────────────────────────────────────────────────────────────
        Task WriteJobQueueLogAsync(
            Guid jobQueueId, string note, string? actor, CancellationToken ct = default);

        Task<IReadOnlyList<BatchJobQueueLog>> GetJobQueueLogsAsync(
            Guid jobQueueId, CancellationToken ct = default);

        // ── Staging — replace semantics (delete-then-insert within transaction) ──
        Task ReplaceStagingFecAsync(
            Guid jobQueueId,
            IReadOnlyList<TestOrProductStagingRow> fecRows,
            IReadOnlyList<TestRequirementStagingRow> agrupRows,
            CancellationToken ct = default);

        Task ReplaceStagingStaffAsync(
            Guid jobQueueId,
            IReadOnlyList<ProfitCentreGradeStagingRow> rows,
            CancellationToken ct = default);

        Task ReplaceStagingAnimalAsync(
            Guid jobQueueId,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default);

        /// <summary>Deletes all staging rows for the given request. Used on cancellation.</summary>
        Task ClearStagingByJobQueueIdAsync(
            Guid jobQueueId, string jobName, CancellationToken ct = default);

        Task<IReadOnlyList<TestOrProductStagingRow>> GetTestOrProductStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);
        Task<IReadOnlyList<TestRequirementStagingRow>> GetTestRequirementStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);
        Task<IReadOnlyList<ProfitCentreGradeStagingRow>> GetProfitCentreGradeStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);
        Task<IReadOnlyList<AnimalStagingRow>> GetAnimalStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);

        // ── Validation errors ────────────────────────────────────────────────────
        Task ReplaceValidationErrorsAsync(
            Guid jobQueueId,
            IReadOnlyList<StagingValidationError> errors,
            CancellationToken ct = default);

        Task<IReadOnlyList<StagingValidationError>> GetValidationErrorsAsync(
            Guid jobQueueId, CancellationToken ct = default);

        /// <summary>
        /// Atomically sets status to Cancelled and deletes all staging rows for the request.
        /// Used exclusively for the Cancel workflow where both must succeed or neither should.
        /// </summary>
        Task CancelAndClearStagingAsync(
            Guid jobQueueId, string jobName,
            string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default);

        // ── Reference checks (used during upload validation) ─────────────────────
        /// <summary>
        /// Returns the fps.tblyearmaster yearstatus for the given year (e.g. "Open", "Planned",
        /// "Closed"), or null if the year doesn't exist or isn't active.
        /// </summary>
        Task<string?> GetFpsYearStatusAsync(int fpsYear, CancellationToken ct = default);

        /// <summary>
        /// Bulk check for ValidationContext.ProjectLookup: returns the subset of
        /// fps.tlkpproject.parentproject codes that exist for the given year.
        /// </summary>
        Task<IReadOnlySet<string>> GetExistingProjectCodesAsync(
            IEnumerable<string> parentProjectCodes, int fpsYear, CancellationToken ct = default);

        /// <summary>
        /// Bulk check for ValidationContext.CapabilityLookup: returns
        /// the subset of (testCode, workGroup) pairs that exist in fps.tlkptestcapability for
        /// the given year.
        /// </summary>
        Task<IReadOnlySet<(string TestCode, string WorkGroup)>> GetExistingCapabilityPairsAsync(
            IEnumerable<(string TestCode, string WorkGroup)> pairs, int fpsYear, CancellationToken ct = default);

        // ── Download snapshot ─────────────────────────────────────────────────────

        /// <summary>Next monotonic download_version for this request (1 if none exist yet).</summary>
        Task<int> GetNextDownloadVersionAsync(Guid jobQueueId, CancellationToken ct = default);

        /// <summary>
        /// Steps 1-2: creates the download_version header as 'Generating' and persists
        /// the immutable snapshot rows (keys, source rates, and the descriptive fields the
        /// workbook needs to render) in one transaction.
        /// </summary>
        Task CreateDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<TestOrProductStagingRow> fecRows, IReadOnlyList<TestRequirementStagingRow> agrupRows,
            CancellationToken ct = default);

        /// <summary>
        /// Step 4: marks the header 'Ready' and sets job_queue.active_download_version,
        /// in one transaction — only called after the workbook has been generated successfully.
        /// </summary>
        Task MarkDownloadReadyAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>
        /// Best-effort: marks the header 'Failed' if workbook generation throws after the
        /// snapshot already committed. active_download_version is deliberately left untouched
        /// so the previous, still-valid version remains the one an upload is
        /// checked against.
        /// </summary>
        Task MarkDownloadFailedAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>
        /// Step 3: reads back the just-persisted snapshot rows for a download version —
        /// never a live requery of fps.testorproduct.
        /// </summary>
        Task<IReadOnlyList<TestOrProductStagingRow>> GetFecSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>As GetFecSnapshotRowsAsync, for AGRUP — never a live requery of fps.tlkptestreqmt.</summary>
        Task<IReadOnlyList<TestRequirementStagingRow>> GetAgrupSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>
        /// Staff equivalent of CreateDownloadSnapshotAsync. Persists
        /// to fps.bulk_rates_staff_download_detail — a dedicated table, not a widened
        /// fps.bulk_rates_downloaded_key, since that table is hard-restricted to FEC/AGRUP by
        /// chk_bulk_rates_downloaded_key_sheetname (confirmed live). Both this and
        /// CreateAnimalDownloadSnapshotAsync reuse the shared fps.bulk_rates_download header via
        /// the same composite-FK pattern CreateDownloadSnapshotAsync already uses.
        /// </summary>
        Task CreateStaffDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<ProfitCentreGradeStagingRow> rows,
            CancellationToken ct = default);

        /// <summary>Staff equivalent of GetFecSnapshotRowsAsync — reads back fps.bulk_rates_staff_download_detail.</summary>
        Task<IReadOnlyList<ProfitCentreGradeStagingRow>> GetStaffSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>Animal equivalent of CreateDownloadSnapshotAsync. Persists to fps.bulk_rates_animal_download_detail.</summary>
        Task CreateAnimalDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default);

        /// <summary>Animal equivalent of GetFecSnapshotRowsAsync — reads back fps.bulk_rates_animal_download_detail.</summary>
        Task<IReadOnlyList<AnimalStagingRow>> GetAnimalSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        // ── Export (live table reads for Excel download) ──────────────────────────
        Task<IReadOnlyList<TestOrProductStagingRow>> GetFecRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<TestRequirementStagingRow>> GetAgrupRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<ProfitCentreGradeStagingRow>> GetStaffRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<AnimalStagingRow>> GetAnimalRowsForExportAsync(int fpsYear, CancellationToken ct = default);

        // ── Freeze reviewed classification onto staging ──────────────────────────

        /// <summary>
        /// Writes the classification computed at release time onto the matching
        /// FEC/AGRUP staging rows' calculated_action/effective_new_rate/source_current_rate/
        /// validation_version columns, keyed by business key (TestCode for FEC,
        /// TestCode+Buyer for AGRUP) — never by source row number, which is not stable across
        /// a DB read-back. Called once, at release, so the worker's
        /// revalidation has a frozen baseline to detect drift against.
        /// </summary>
        Task FreezeStagingCalculatedActionsAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<TestFreezeEntry> fecFreezes,
            IReadOnlyList<TestFreezeEntry> agrupFreezes,
            CancellationToken ct = default);

        /// <summary>
        /// Staff equivalent of FreezeStagingCalculatedActionsAsync —
        /// writes the release-time reviewed classification onto fps.tblstagingprofitcentregrade's
        /// source_*/effective_*/calculated_action/validation_version columns, keyed by
        /// PcGrade.
        /// </summary>
        Task FreezeStaffStagingAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<StaffFreezeEntry> freezes,
            CancellationToken ct = default);

        /// <summary>As FreezeStaffStagingAsync, for Animal — fps.tblstaginganimals, keyed by AnimalType.</summary>
        Task FreezeAnimalStagingAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<AnimalFreezeEntry> freezes,
            CancellationToken ct = default);
    }
}
