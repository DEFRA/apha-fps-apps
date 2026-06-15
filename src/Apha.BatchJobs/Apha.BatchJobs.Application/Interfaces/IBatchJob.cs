namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>Contract for all batch job implementations.</summary>
public interface IBatchJob
{
    /// <summary>Gets the unique name of the batch job.</summary>
    string Name { get; }

    /// <summary>
    /// Gets the explicit idempotency strategy used by this job.
    /// Examples: Upsert, DedupKey, Checkpointing.
    /// </summary>
    string IdempotencyStrategy { get; }

    /// <summary>
    /// Gets the optional schedule expression for EventBridge Scheduler.
    /// Format: cron expression (e.g., "0 20 ? * MON-SAT" for Mon-Sat 8pm).
    /// Set to null for auto/user-triggered jobs (RecreateSummaries, FECProcess).
    /// </summary>
    string? ScheduleExpression => null;

    /// <summary>
    /// Gets the optional human-readable schedule description.
    /// Used for documentation and operational clarity.
    /// </summary>
    string? ScheduleDescription => null;

    /// <summary>
    /// Gets the optional maximum execution timeout in seconds.
    /// If not specified, defaults to BatchJobs:JobTimeout from configuration.
    /// </summary>
    int? MaxExecutionSeconds => null;

    /// <summary>Executes the batch job asynchronously.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}