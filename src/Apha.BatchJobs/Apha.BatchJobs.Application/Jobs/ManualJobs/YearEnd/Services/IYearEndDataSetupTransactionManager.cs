namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Runs the full Year End Data Setup step pipeline in one transaction — all steps commit together,
/// or all roll back if any step throws.
/// </summary>
public interface IYearEndDataSetupTransactionManager
{
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
