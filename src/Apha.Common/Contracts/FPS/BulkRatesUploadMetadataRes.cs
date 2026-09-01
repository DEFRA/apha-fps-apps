namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a Bulk Rates request's upload/validation metadata.
    /// </summary>
    public class BulkRatesUploadMetadataRes
    {
        public string? Filename { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCountsRes RowCounts { get; set; } = new();
    }
}
