using Apha.BatchJobs.Infrastructure.Data;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Represents one legacy MABArchive year-load step.
/// </summary>
public interface IMabArchiveLoader
{
    /// <summary>
    /// Legacy sequence position from sp_AddYearsFPSData.
    /// </summary>
    int Sequence { get; }

    /// <summary>
    /// Logical loader name used in logs.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes this loader for the supplied year.
    /// </summary>
    Task<int> LoadAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}
