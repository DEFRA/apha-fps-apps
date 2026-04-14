namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Enumeration of job execution statuses.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job is waiting to be executed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Job is currently executing.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Job failed with an error.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Job was cancelled before completion.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Job retry is scheduled.
    /// </summary>
    Retry = 5,

    /// <summary>
    /// Job was skipped because a concurrent run already holds the distributed lock.
    /// </summary>
    Skipped = 6
}
