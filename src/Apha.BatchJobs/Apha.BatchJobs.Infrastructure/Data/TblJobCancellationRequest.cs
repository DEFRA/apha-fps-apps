namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>
/// EF entity for fps.job_cancellation_request.
/// </summary>
internal sealed class TblJobCancellationRequest
{
    public Guid JobExecutionId { get; set; }

    public required string RequestedBy { get; set; }

    public DateTime RequestedAtUtc { get; set; }

    public required string Status { get; set; }

    public string? Source { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }

    public string? ConsumedBy { get; set; }

    public DateTime? TerminalizedAtUtc { get; set; }
}
