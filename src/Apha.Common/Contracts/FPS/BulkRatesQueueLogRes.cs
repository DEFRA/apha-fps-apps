namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a Bulk Rates job_queue_log entry.
    /// </summary>
    public class BulkRatesQueueLogRes
    {
        public long LogId { get; set; }
        public Guid JobQueueId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Actor { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
