namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Enumeration of job execution statuses.
/// Current lifecycle: Initiated -> Running -> Completed | Failed.
/// Legacy value Cancelled is retained for backward compatibility with historical rows.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// API accepted the trigger and created the job queue record before publishing to EventBridge.
    /// </summary>
    Initiated = 0,

    /// <summary>
    /// Worker has acquired the lock and is actively executing the job.
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
    /// Legacy status retained for historical compatibility.
    /// </summary>
    Cancelled = 4
}
