namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// Execution context for MABArchive load operations.
/// </summary>
public sealed record MabArchiveExecutionContext(
    int CurrentYear,
    int PreviousYear,
    int CurrentMonth,
    int PrimaryYear,
    bool IncludePartialRefreshYear)
{
    /// <summary>
    /// Determines if the current month requires a partial refresh (month Γëñ 4).
    /// </summary>
    public bool RequiresPartialRefresh => CurrentMonth <= 4;

    /// <summary>
    /// The year for partial refresh (current year when month Γëñ 4).
    /// </summary>
    public int? PartialRefreshYear => RequiresPartialRefresh ? CurrentYear : null;
}
