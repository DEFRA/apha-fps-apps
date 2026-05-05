namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Service for managing yearly FPS archive data operations.
/// </summary>
public interface IMyFpsYearlyDataService
{
    /// <summary>
    /// Checks whether the specified FPS year is registered and available for processing.
    /// </summary>
    /// <param name="year">The FPS year to check. When null, uses the scoped execution year context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the year is available; otherwise false.</returns>
    Task<bool> IsYearAvailableAsync(int? year, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all archive data for the specified year in dependency order.
    /// </summary>
    /// <param name="year">The year to delete archive data for. When null, uses the scoped execution year context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> DeleteYearDataAsync(int? year, CancellationToken cancellationToken);

    /// <summary>
    /// Loads fresh archive data from FPS source for the specified year in dependency order.
    /// </summary>
    /// <param name="year">The year to load archive data for. When null, uses the scoped execution year context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> LoadYearDataAsync(int? year, CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes only the my_tlkpproject_all cross-reference table for the specified year.
    /// Used for partial refresh when month ≤ 4.
    /// </summary>
    /// <param name="year">The year to refresh project all for. When null, uses the scoped execution year context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> RefreshProjectAllOnlyAsync(int? year, CancellationToken cancellationToken);
}
