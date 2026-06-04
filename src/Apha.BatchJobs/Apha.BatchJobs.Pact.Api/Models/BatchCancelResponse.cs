namespace Apha.BatchJobs.Pact.Api.Models;

public enum BatchCancellationStatus
{
    Accepted,
    AlreadyRequested,
    NoOpTerminal
}

public sealed class BatchCancelResponse
{
    public required string JobName { get; init; }

    public required string JobExecutionId { get; init; }

    public required BatchCancellationStatus CancellationStatus { get; init; }

    public bool Accepted { get; init; }

    public bool AlreadyRequested { get; init; }

    public bool NoOpTerminal { get; init; }

    public string? ExecutionState { get; init; }

    public string? Message { get; init; }
}
