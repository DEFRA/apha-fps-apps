namespace Apha.FPS.Core.Entities
{
    public class BatchJobQueue
    {
        public Guid JobqueueId { get; set; }
        public Guid JobExecutionId { get; set; }
        public int JobId { get; set; }
        public int StatusId { get; set; } = 0;
        public string RequestedBy { get; set; } = null!;
        public DateTime? RequestedAtUtc { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int FpsYear { get; set; }

        // Lifecycle metadata columns already exist in fps.job_queue (consumed by
        // Apha.BatchJobs.JobOrchestrator.ValidateApprovalMetadataAsync) but were never mapped or
        // written here - see fps-year-end-phase6-implementation-trace-2026-08-15.md, "Urgent
        // finding". Added 2026-08-15 so approval/rejection actually populate them.
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public string? RejectionReason { get; set; }
        public string? TriggeredBy { get; set; }
        public DateTime? TriggeredAtUtc { get; set; }
    }
}
