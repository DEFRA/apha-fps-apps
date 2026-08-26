namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Scoped execution context for FPS year selection during a batch job run.
/// Mirrors the API-side pattern where year is resolved once and consumed downstream.
/// </summary>
public interface IExecutionYearContext
{
    /// <summary>
    /// Gets or sets the active FPS year for the current execution scope.
    /// </summary>
    int? FpsYear { get; set; }

    /// <summary>
    /// Gets or sets the source used to resolve the current year value.
    /// </summary>
    string YearSource { get; set; }
}
