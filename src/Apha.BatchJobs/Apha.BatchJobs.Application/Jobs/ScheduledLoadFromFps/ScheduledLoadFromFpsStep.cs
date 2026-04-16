namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Logical orchestration steps for the ScheduledLoadFromFps workflow.
/// </summary>
public enum ScheduledLoadFromFpsStep
{
    /// <summary>
    /// Builds totals for the previous FPS year.
    /// </summary>
    ProcessPreviousYearTotals = 1,

    /// <summary>
    /// Builds totals for the current FPS year when cutover month is passed.
    /// </summary>
    ProcessCurrentYearTotals = 2,

    /// <summary>
    /// Deletes year-scoped archive data before reload.
    /// </summary>
    DeleteYearsFpsData = 3,

    /// <summary>
    /// Adds year-scoped archive data after delete phase.
    /// </summary>
    AddYearsFpsData = 4,

    /// <summary>
    /// Handles MY_tlkpProject_All refresh path for the current year.
    /// </summary>
    HandleCurrentYearProjectAll = 5
}
