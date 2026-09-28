using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Operational.Repositories;

/// <inheritdoc cref="IHeartbeatRepositoryScope"/>
internal sealed class HeartbeatRepositoryScope : IHeartbeatRepositoryScope
{
    private readonly BatchJobsDbContext _context;

    public HeartbeatRepositoryScope(BatchJobsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        LockRepository = new BatchLockRepository(_context);
        ExecutionRepository = new JobExecutionRepository(_context);
    }

    public IBatchLockRepository LockRepository { get; }

    public IJobExecutionRepository ExecutionRepository { get; }

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}

/// <inheritdoc cref="IHeartbeatRepositoryScopeFactory"/>
public sealed class HeartbeatRepositoryScopeFactory : IHeartbeatRepositoryScopeFactory
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;

    public HeartbeatRepositoryScopeFactory(IDbContextFactory<BatchJobsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public IHeartbeatRepositoryScope Create() => new HeartbeatRepositoryScope(_dbContextFactory.CreateDbContext());
}
