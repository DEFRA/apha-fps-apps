using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.YearEnd.Repositories;

/// <summary>
/// Mirrors <c>MabArchiveTransactionManager</c>'s shape exactly. Depends on the same scoped
/// <see cref="BatchJobsDbContext"/> instance <see cref="YearEndDataSetupRepository"/> uses, so
/// beginning a transaction here makes every repository call made during the wrapped operation (via
/// any <c>IYearEndDataSetupStep</c>) participate in it automatically.
/// </summary>
internal sealed class YearEndDataSetupTransactionManager : IYearEndDataSetupTransactionManager
{
    private readonly BatchJobsDbContext _context;

    public YearEndDataSetupTransactionManager(BatchJobsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
