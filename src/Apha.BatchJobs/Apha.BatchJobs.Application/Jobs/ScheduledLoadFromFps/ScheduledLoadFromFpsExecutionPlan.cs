namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Immutable execution plan for ScheduledLoadFromFps.
/// </summary>
/// <param name="Context">Execution context used to build the plan.</param>
/// <param name="Steps">Ordered list of logical orchestration steps.</param>
public sealed record ScheduledLoadFromFpsExecutionPlan(
    ScheduledLoadFromFpsExecutionContext Context,
    IReadOnlyList<ScheduledLoadFromFpsStep> Steps);
