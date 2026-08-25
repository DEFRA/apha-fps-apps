using Apha.FPS.Application.Common.BulkRates;

namespace Apha.FPS.Application.Validation.BulkRates
{
    /// <summary>
    /// Input. Everything <see cref="IBulkRatesValidationService"/> needs, pre-fetched by the
    /// caller (<see cref="BulkRatesValidator"/>) — the service itself is side-effect free and
    /// does no I/O, so the same context always produces the same result (determinism).
    /// Lookups must be built via <see cref="BulkRatesValidationKeys"/> so key
    /// normalization matches between context construction and validation.
    /// </summary>
    public sealed record ValidationContext
    {
        public required Guid JobQueueId { get; init; }
        public required int FpsYear { get; init; }

        /// <summary>Null when no download has happened yet for this request.</summary>
        public int? DownloadVersion { get; init; }

        public required int UploadVersion { get; init; }

        /// <summary>Key: <see cref="BulkRatesValidationKeys.TestCode"/>. Every live fps.testorproduct row for TestCodes relevant to this upload.</summary>
        public required IReadOnlyDictionary<string, LiveFecRow> LiveFecLookup { get; init; }

        /// <summary>Key: <see cref="BulkRatesValidationKeys.AgrupKey"/>.</summary>
        public required IReadOnlyDictionary<(string TestCode, string Buyer), LiveAgrupRow> LiveAgrupLookup { get; init; }

        /// <summary>Existing fps.tlkpproject.parentproject codes for FpsYear, keyed by <see cref="BulkRatesValidationKeys.TestCode"/> (reused for any single-string business code).</summary>
        public required IReadOnlySet<string> ProjectLookup { get; init; }

        /// <summary>Existing fps.tlkptestcapability (TestCode, WorkGroup) pairs for FpsYear. Key: <see cref="BulkRatesValidationKeys.CapabilityKey"/>.</summary>
        public required IReadOnlySet<(string TestCode, string WorkGroup)> CapabilityLookup { get; init; }

        public required IReadOnlyList<ValidationFecRow> StagedFecRows { get; init; }
        public required IReadOnlyList<ValidationAgrupRow> StagedAgrupRows { get; init; }

        /// <summary>fps.bulk_rates_downloaded_key rows for DownloadVersion. Empty (not null) when DownloadVersion is null.</summary>
        public required IReadOnlyList<DownloadedSnapshotKey> FrozenSnapshot { get; init; }

        /// <summary>
        /// Gates the BC-05 live-vs-snapshot-absence check — a live positive AGRUP row,
        /// under a FEC TestCode being withdrawn, that was never part of the download snapshot.
        /// The API's own release-time check is deliberately scoped to snapshot-known rows only
        /// ("a live AGRUP row created after download... cannot be caught here"): a row invisible
        /// at download time is also invisible to whoever is reviewing the release, so blocking
        /// release over it would be unreviewable, not just unhelpful. Every current FPS call
        /// site leaves this false, so this check does not currently run anywhere — retained
        /// for the possibility of a future call site that legitimately needs it, not dead code
        /// to remove casually.
        /// </summary>
        public bool IncludeWorkerOnlyChecks { get; init; }
    }
}
