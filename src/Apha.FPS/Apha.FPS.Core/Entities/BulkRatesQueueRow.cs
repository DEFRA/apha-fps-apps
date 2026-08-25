namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Joined query projection over fps.job_queue + fps.job_master + fps.job_status
    /// (JobName/Status come from the join, not job_queue itself) — not a physical-table
    /// entity or a database view; see <c>ResourceStaffJobDetailRow</c>/<c>ResourceMgmtReplanRow</c>
    /// for the established *Row precedent this follows. Used by the FPS API for all Bulk Rates
    /// lifecycle operations. Deliberately kept separate from <see cref="BatchJobQueue"/> (the
    /// narrow EF entity for fps.job_queue) rather than extending it, since most of these fields
    /// are Bulk-Rates-specific workflow columns irrelevant to that entity's other consumers.
    /// </summary>
    public class BulkRatesQueueRow
    {
        public Guid JobQueueId { get; set; }
        public int JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid JobExecutionId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public int FpsYear { get; set; }
        public string? UploadFilename { get; set; }
        public string? UploadChecksumSha256 { get; set; }
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
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FailureReason { get; set; }
        /// <summary>
        /// The download version that the next upload must carry in its protected
        /// metadata sheet. Null means no workbook has been generated yet for this request.
        /// </summary>
        public int? ActiveDownloadVersion { get; set; }
        /// <summary>
        /// S3 object key of the latest uploaded Bulk Rates workbook artefact for this request.
        /// Prior versioned artefacts remain retained in S3 under their versioned keys but are
        /// not individually enumerated here.
        /// </summary>
        public string? S3ObjectKey { get; set; }
    }
}
