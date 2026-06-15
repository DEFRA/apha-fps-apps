namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Outcome of a single RecreateSummaries execution step.
/// </summary>
public enum StepStatus
{
    /// <summary>Step completed without error.</summary>
    Success,

    /// <summary>Step threw an exception or reported a database error.</summary>
    Failed,

    /// <summary>Step was intentionally not executed (e.g. period locked).</summary>
    Skipped
}
