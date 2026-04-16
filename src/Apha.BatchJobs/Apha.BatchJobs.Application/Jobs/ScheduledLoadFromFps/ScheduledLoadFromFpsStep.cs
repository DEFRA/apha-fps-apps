namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Logical orchestration steps for the ScheduledLoadFromFps workflow.
/// </summary>
public enum ScheduledLoadFromFpsStep
{
    ProcessPreviousYearTotals = 1,
    ProcessCurrentYearTotals = 2,
    DeleteYearsFpsData = 3,
    AddYearsFpsData = 4,
    HandleCurrentYearProjectAll = 5
}
