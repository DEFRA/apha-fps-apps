namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Computed context used by ScheduledLoadFromFps execution planning and future DB execution.
/// </summary>
/// <param name="CurrentMonth">UTC month used for branching decisions.</param>
/// <param name="CurrentYear">Current FPS year.</param>
/// <param name="PreviousYear">Previous FPS year.</param>
/// <param name="CurrentYearCutoverMonth">Cutover month used by the orchestration logic.</param>
public sealed record ScheduledLoadFromFpsExecutionContext(
    int CurrentMonth,
    int CurrentYear,
    int PreviousYear,
    int CurrentYearCutoverMonth);
