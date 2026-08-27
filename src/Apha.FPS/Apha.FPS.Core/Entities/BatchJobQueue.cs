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
        // Populated at creation by the producer (Year End / Recreate Summary / Bulk Rates);
        // overwritten with the actual execution start when the Batch Worker's Running
        // transition runs (JobExecutionRepository — Apha.BatchJobs.Infrastructure.Operational.Repositories).
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int FpsYear { get; set; }

        // ── Bulk Rates workflow columns ──────────────────────────────────────────
        // Added for BulkRatesRepository's LINQ conversion rather than a second entity
        // mapped to this table — EF Core rejects two unrelated entity types sharing one
        // table without an explicit linking relationship.
        //
        // Not all of these are Bulk-Rates-exclusive. ApprovedBy/ApprovedAtUtc/RejectedBy/
        // RejectedAtUtc/TriggeredBy/TriggeredAtUtc are shared lifecycle/audit columns —
        // Year End's own approval/cutover flow already reads and writes these via the
        // BatchJobs worker's JobExecutionRepository (CR025). The rest (UploadFilename,
        // UploadVersion, UploadValidatedAtUtc, UploadRowCountsJson, CancelledBy,
        // CancelledAtUtc, CancellationReason, ActiveDownloadVersion) are Bulk-Rates-only;
        // YearEnd never sets or reads those and they stay null for every row it creates.
        public string? UploadFilename { get; set; }
        public int? UploadVersion { get; set; }
        public DateTime? UploadValidatedAtUtc { get; set; }
        public string? UploadRowCountsJson { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string? CancellationReason { get; set; }
        public string? TriggeredBy { get; set; }
        public DateTime? TriggeredAtUtc { get; set; }
        public int? ActiveDownloadVersion { get; set; }
    }
}
