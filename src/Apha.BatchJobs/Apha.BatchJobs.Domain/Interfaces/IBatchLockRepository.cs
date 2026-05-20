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
    /// <param name="jobQueueId">Unique UUID for the job queue entry (caller-injected).</param>
    /// <param name="timeoutSeconds">Lock timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if lock acquired; false if already locked by another process.</returns>
    Task<bool> TryAcquireLockAsync(string jobName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a lock held by the given job queue ID.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="jobQueueId">The job queue UUID holding the lock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReleaseLockAsync(string jobName, Guid jobQueueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current lock for a job, if one exists.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default);
}
