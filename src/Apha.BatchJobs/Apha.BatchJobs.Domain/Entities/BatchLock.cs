namespace Apha.BatchJobs.Domain.Entities;

/// <summary>Represents a distributed lock for batch job execution to prevent concurrent runs.</summary>
public sealed class BatchLock
{
    /// <summary>Unique identifier for the lock.</summary>
    public required int LockId { get; set; }

    /// <summary>Name of the batch job.</summary>
    public required string JobName { get; set; }

    /// <summary>Timestamp when the lock was acquired.</summary>
    public required DateTime AcquiredAt { get; set; }

    /// <summary>Timestamp when the lock should be released (timeout).</summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>The UUID for the job queue entry holding this lock (caller-injected).</summary>
    public required Guid JobQueueId { get; set; }

    /// <summary>Whether the lock is currently active.</summary>
    public required bool IsActive { get; set; }
}
