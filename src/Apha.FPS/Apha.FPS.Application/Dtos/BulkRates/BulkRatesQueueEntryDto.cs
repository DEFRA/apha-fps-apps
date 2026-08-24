namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// API/JSON contract shape for a Bulk Rates job_queue entry. Mirrors every property of
    /// <see cref="Apha.FPS.Core.Entities.BulkRatesQueueRow"/> exactly (wire-preserving —
    /// this is a boundary correction, not a contract trim) so Core persistence types are no
    /// longer serialized directly by the API.
    /// </summary>
    public class BulkRatesQueueEntryDto
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
        public int? ActiveDownloadVersion { get; set; }
        public string? S3ObjectKey { get; set; }
    }
}
