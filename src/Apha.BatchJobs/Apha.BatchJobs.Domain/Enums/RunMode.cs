namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Enumeration of batch job execution modes.
/// </summary>
public enum RunMode
{
    /// <summary>
    /// Job runs on a predefined schedule via EventBridge.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Job is triggered manually on-demand (via UI, API, or CLI).
    /// </summary>
    Manual = 1
}
