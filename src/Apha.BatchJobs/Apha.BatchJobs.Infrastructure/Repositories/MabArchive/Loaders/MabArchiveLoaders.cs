using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal abstract class MabArchiveLoaderBase : IMabArchiveLoader
{
    public abstract int Sequence { get; }

    public abstract string Name { get; }

    public Task<int> LoadAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return ExecuteAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}

internal abstract class MabArchiveDotNetLoaderBase : MabArchiveLoaderBase
{
    protected override Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return LoadWithDotNetAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> LoadWithDotNetAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}
