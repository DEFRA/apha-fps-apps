namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Persistence contract for Year End Cutover year-status transitions.
/// </summary>
public interface IYearEndCutoverRepository
{
    /// <summary>
    /// Opens a transaction, locks both year rows FOR UPDATE, validates their states,
    /// closes the current year and opens the target year, then commits.
    /// </summary>
    Task ExecuteCutoverAsync(int currentYear, int targetYear, CancellationToken cancellationToken = default);
}
