namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Persistence contract for Year End Data Setup operations.
/// </summary>
public interface IYearEndDataSetupRepository
{
    /// <summary>
    /// Checks whether a year row exists in fps.tblyearmaster and returns its status and active flag,
    /// or null if no row exists for the given year.
    /// </summary>
    Task<(string YearStatus, bool Active)?> GetYearStateAsync(int fpsYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new row in fps.tblyearmaster with Planned status and returns the number of rows affected.
    /// </summary>
    Task<int> InsertPlannedYearAsync(int fpsYear, string fpsYearCode, string correlationId, CancellationToken cancellationToken = default);
}
