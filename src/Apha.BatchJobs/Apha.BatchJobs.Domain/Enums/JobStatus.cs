namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Enumeration of job execution statuses.
/// Current lifecycle: Initiated -> Approved -> Running -> Completed | Failed.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// API accepted the trigger and created the job queue record before publishing to EventBridge.
    /// </summary>
    Initiated = 0,

    /// <summary>
    /// Request approved and eligible for worker trigger.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Worker has acquired the lock and is actively executing the job.
    /// </summary>
    Running = 2,

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Job failed with an error.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Request rejected by an approver and will not be executed.
    /// </summary>
    Rejected = 5
}
