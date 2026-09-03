namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a single Bulk Rates request's full detail — the queue entry plus its
    /// upload metadata and lifecycle log. Returned by create/release/approve/reject/cancel/get.
    /// </summary>
    public class BulkRatesRequestDetailRes
    {
        public BulkRatesQueueEntryRes Entry { get; set; } = null!;
        public BulkRatesUploadMetadataRes? UploadMetadata { get; set; }
        public IReadOnlyList<BulkRatesQueueLogRes> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }
}
