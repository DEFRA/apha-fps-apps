using Apha.BatchJobs.Domain.Entities;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Repository for managing batch job execution locks.
/// </summary>
public interface IBatchLockRepository
{
    /// <summary>
    /// Attempts to acquire a distributed lock for a job.
    /// </summary>
    /// <param name="jobName">The name of the job requiring a lock.</param>
    /// <param name="runId">Unique run ID for this execution.</param>
    /// <param name="timeoutSeconds">Lock timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if lock acquired; false if already locked by another process.</returns>
    Task<bool> TryAcquireLockAsync(string jobName, string runId, int timeoutSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a lock held by the given run ID.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="runId">The run ID holding the lock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReleaseLockAsync(string jobName, string runId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current lock for a job, if one exists.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default);
}
