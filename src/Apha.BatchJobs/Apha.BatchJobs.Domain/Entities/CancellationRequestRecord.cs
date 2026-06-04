namespace Apha.BatchJobs.Domain.Entities;

/// <summary>
/// Durable cancellation request details keyed by JobExecutionId.
/// </summary>
public sealed class CancellationRequestRecord
{
    public Guid JobExecutionId { get; init; }

    public required string RequestedBy { get; init; }

    public DateTime RequestedAtUtc { get; init; }

    public string Status { get; init; } = "Pending";

    public DateTime? ConsumedAtUtc { get; init; }

    public string? ConsumedBy { get; init; }

    public DateTime? TerminalizedAtUtc { get; init; }
}
