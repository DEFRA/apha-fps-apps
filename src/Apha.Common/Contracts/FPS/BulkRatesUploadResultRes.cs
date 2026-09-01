namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for the result of a Bulk Rates file upload or validation-results query.
    /// </summary>
    public class BulkRatesUploadResultRes
    {
        public Guid JobQueueId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UploadVersion { get; set; }
        public string? Filename { get; set; }
        public BulkRatesRowCountsRes RowCounts { get; set; } = new();
        public IReadOnlyList<BulkRatesValidationErrorRes> ValidationErrors { get; set; } = [];
    }
}
