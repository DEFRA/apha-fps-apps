namespace Apha.PACT.Core.Entities
{
    public class BatchJobHistory
    {
        public int JobId { get; set; }
        public string JobName { get; set; } = null!;
        public Guid JobExecutionId { get; set; }
        public string Status { get; set; } = null!;
        public string RequestedBy { get; set; } = null!;
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Original request context, sourced from the Initiated <c>job_queue_log</c> row for this
        /// request — never any other status's note. <c>null</c> when no Initiated log row exists.
        /// </summary>
        public string? Remarks { get; set; }
    }
}
