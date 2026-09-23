using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.YearEnd.Repositories;

/// <summary>
/// Depends on the same scoped <see cref="BatchJobsDbContext"/> instance
/// <see cref="YearEndDataSetupRepository"/> uses, so beginning a transaction here makes every
/// repository call made during the wrapped operation (via any <c>IYearEndDataSetupStep</c>)
/// participate in it automatically.
/// </summary>
internal sealed class YearEndDataSetupTransactionManager : IYearEndDataSetupTransactionManager
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<YearEndDataSetupTransactionManager> _logger;

    public YearEndDataSetupTransactionManager(BatchJobsDbContext context, ILogger<YearEndDataSetupTransactionManager> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);

            if (IsDebugDryRun)
            {
                _logger.LogWarning("YEAR_END_DEBUG_DRY_RUN is set — rolling back Data Setup instead of committing.");
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            await transaction.CommitAsync(cancellationToken);
        });
    }

    // Debug-only escape hatch so a local/manual run can exercise the full pipeline against a real
    // database without persisting — never set in a deployed environment.
    private static bool IsDebugDryRun =>
        string.Equals(Environment.GetEnvironmentVariable("YEAR_END_DEBUG_DRY_RUN"), "true", StringComparison.OrdinalIgnoreCase);
}
