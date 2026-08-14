using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Data Setup. Owns the single connection/transaction
/// shared by every step in the pipeline; commits only after the final step succeeds, and rolls
/// back the entire pipeline on any failure so no partial business-data commit can occur.
/// </summary>
public sealed class YearEndDataSetupService : IYearEndDataSetupService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<YearEndDataSetupService> _logger;
    private readonly IReadOnlyList<IYearEndDataSetupStep> _steps;

    public YearEndDataSetupService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IEnumerable<IYearEndDataSetupStep> steps,
        ILogger<YearEndDataSetupService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _steps = steps?.ToList() ?? throw new ArgumentNullException(nameof(steps));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "YearEndDataSetup service contract invoked | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            context.TargetFpsYear,
            context.CurrentFpsYear);

        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("Year End Data Setup has no registered execution steps.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var dbTransaction = transaction.GetDbTransaction();

            try
            {
                var runningContext = context;

                for (var i = 0; i < _steps.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var step = _steps[i];
                    var startedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "YearEndDataSetup step started | CorrelationId={CorrelationId} | StepIndex={StepIndex} | StepCount={StepCount} | StepName={StepName}",
                        runningContext.CorrelationId,
                        i + 1,
                        _steps.Count,
                        step.Name);

                    runningContext = await step.ExecuteAsync(runningContext, connection, dbTransaction, cancellationToken);

                    var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
                    _logger.LogInformation(
                        "YearEndDataSetup step completed | CorrelationId={CorrelationId} | StepIndex={StepIndex} | StepCount={StepCount} | StepName={StepName} | DurationMs={DurationMs}",
                        runningContext.CorrelationId,
                        i + 1,
                        _steps.Count,
                        step.Name,
                        elapsedMs);
                }

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "YearEndDataSetup pipeline completed | CorrelationId={CorrelationId}",
                    runningContext.CorrelationId);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });
    }
}
