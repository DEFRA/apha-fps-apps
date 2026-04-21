using Apha.BatchJobs.Domain.Entities;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Provides job status information for the API layer.
/// Allows the UI to check if a job is currently running before triggering it.
/// </summary>
public interface IJobStatusService
{
    /// <summary>
    /// Gets the current status of a batch job, including whether a lock is held
    /// and the details of the last execution.
    /// </summary>
    /// <param name="jobName">Name of the batch job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobStatusResult> GetStatusAsync(string jobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status for all registered batch jobs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<JobStatusResult>> GetAllStatusesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the current status of a batch job.
/// </summary>
public sealed class JobStatusResult
{
    /// <summary>Name of the batch job.</summary>
    public required string JobName { get; init; }

    /// <summary>
    /// Whether the job is currently running (lock is held and not expired).
    /// Use this to disable the trigger button in the UI.
    /// </summary>
    public required bool IsRunning { get; init; }

    /// <summary>
    /// The active lock, if held. Null if the job is not running.
    /// </summary>
    public ActiveLockInfo? ActiveLock { get; init; }

    /// <summary>
    /// Summary of the last completed execution, if any.
    /// </summary>
    public LastExecutionInfo? LastExecution { get; init; }
}

/// <summary>Active lock information returned to the UI.</summary>
public sealed class ActiveLockInfo
{
    /// <summary>Unique run ID holding the lock.</summary>
    public required string RunId { get; init; }

    /// <summary>When the lock was acquired.</summary>
    public required DateTime AcquiredAt { get; init; }

    /// <summary>When the lock will expire (safety timeout).</summary>
    public required DateTime ExpiresAt { get; init; }
}

/// <summary>Last execution summary returned to the UI.</summary>
public sealed class LastExecutionInfo
{
    /// <summary>Unique run ID for the last execution.</summary>
    public required string RunId { get; init; }

    /// <summary>Status of the last execution (Completed, Failed, Skipped, etc).</summary>
    public required string Status { get; init; }

    /// <summary>When the last execution started.</summary>
    public required DateTime StartedAt { get; init; }

    /// <summary>When the last execution completed.</summary>
    public DateTime? CompletedAt { get; init; }
}
