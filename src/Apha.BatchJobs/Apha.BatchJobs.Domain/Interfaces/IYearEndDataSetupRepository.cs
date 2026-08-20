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

    /// <summary>Returns true if the specified table exists in information_schema.</summary>
    Task<bool> TableExistsAsync(string schema, string table, CancellationToken cancellationToken = default);

    /// <summary>Returns true if the specified column exists in information_schema.</summary>
    Task<bool> ColumnExistsAsync(string schema, string table, string column, CancellationToken cancellationToken = default);

    /// <summary>Returns true if a row for the given fpsYear exists in fps.tblyearmaster.</summary>
    Task<bool> YearRowExistsAsync(int fpsYear, CancellationToken cancellationToken = default);
}
