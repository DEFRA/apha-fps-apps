namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Wraps the full Year End Data Setup pipeline (all registered <see cref="Steps.IYearEndDataSetupStep"/>
/// invocations) in a single atomic transaction — mirrors
/// <see cref="Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports.IMabArchiveTransactionManager"/>'s
/// shape exactly. All steps succeed and commit together, or any step throws and every mutation made
/// so far in this run rolls back.
/// </summary>
public interface IYearEndDataSetupTransactionManager
{
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
