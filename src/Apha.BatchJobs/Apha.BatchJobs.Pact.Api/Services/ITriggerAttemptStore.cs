namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class TriggerAttemptRecord
{
    public required string JobExecutionId { get; init; }

    public required string JobName { get; init; }

    public required DateTime AcceptedAtUtc { get; init; }

    public required string EventId { get; init; }

    public required bool WorkerProcessLaunched { get; init; }

    public required string Status { get; init; }

    public int? WorkerExitCode { get; init; }

    public string? FailureReason { get; init; }

    public required DateTime StoredAtUtc { get; init; }
}

public interface ITriggerAttemptStore
{
    string StoreName { get; }

    Task SaveAsync(TriggerAttemptRecord record, CancellationToken cancellationToken = default);

    Task<TriggerAttemptRecord?> GetByJobExecutionIdAsync(string jobExecutionId, CancellationToken cancellationToken = default);

    Task<TriggerAttemptRecord?> GetLatestByJobNameAsync(string jobName, CancellationToken cancellationToken = default);
}
