namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Builds deterministic execution plans for ScheduledLoadFromFps runs.
/// </summary>
public interface IScheduledLoadFromFpsPlanBuilder
{
    /// <summary>
    /// Builds an execution plan from current configuration and UTC time.
    /// </summary>
    /// <returns>The immutable execution plan.</returns>
    ScheduledLoadFromFpsExecutionPlan Build();
}
