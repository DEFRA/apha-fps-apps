namespace Apha.BatchJobs.Domain.Entities;

/// <summary>
/// Represents a distributed lock for batch job execution to prevent concurrent runs.
/// </summary>
public sealed class BatchLock
{
    /// <summary>
    /// Unique identifier for the lock.
    /// </summary>
    public required int LockId { get; set; }

    /// <summary>
    /// Name of the batch job.
    /// </summary>
    public required string JobName { get; set; }

    /// <summary>
    /// Timestamp when the lock was acquired.
    /// </summary>
    public required DateTime AcquiredAt { get; set; }

    /// <summary>
    /// Timestamp when the lock should be released (timeout).
    /// </summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The run ID holding this lock.
    /// </summary>
    public required string RunId { get; set; }

    /// <summary>
    /// Whether the lock is currently active.
    /// </summary>
    public required bool IsActive { get; set; }
}
