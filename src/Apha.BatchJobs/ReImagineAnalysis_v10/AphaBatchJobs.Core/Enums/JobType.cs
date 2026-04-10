namespace AphaBatchJobs.Core.Enums;

/// <summary>
/// Defines the type of job trigger mechanism.
/// Used to distinguish between scheduled jobs that run on a cron schedule
/// and adhoc jobs that run on demand.
/// </summary>
public enum JobType
{
    /// <summary>
    /// Represents a job that runs on a predefined schedule (cron-based).
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Represents a job that runs on demand with explicit invocation.
    /// </summary>
    Adhoc = 1
}
