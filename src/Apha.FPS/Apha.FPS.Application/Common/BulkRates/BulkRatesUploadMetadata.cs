using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Common.BulkRates
{
    /// <summary>
    /// Upload/validation summary for a Bulk Rates request, assembled from the typed
    /// upload_* columns on fps.job_queue (configuration_json retired). Application-owned, not
    /// a Core entity: it has no Core-level consumer (never appears in IBulkRatesRepository or
    /// any other Core interface) and is constructed entirely within
    /// BulkRatesRequestService.BuildUploadMetadata from an already-fetched BulkRatesQueueRow —
    /// the repository itself never touches this shape. Colocated with this project's other
    /// BulkRates application helper models (BulkRatesParseResult, BulkRatesJobCapabilities).
    /// RowCounts is the one field still backed by jsonb (upload_row_counts_json),
    /// since it's a purely descriptive nested breakdown with no business-gating role.
    /// </summary>
    public class BulkRatesUploadMetadata
    {
        public string? Filename { get; set; }
        public string? ChecksumSha256 { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCounts RowCounts { get; set; } = new();
    }
}
