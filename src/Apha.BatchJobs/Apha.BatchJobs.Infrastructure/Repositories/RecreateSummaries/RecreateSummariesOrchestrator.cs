using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Orchestrates the ordered execution of all RecreateSummaries steps inside a
/// single PostgreSQL transaction, implements the period-lock branch (Phase 6),
/// and collects per-step results for audit logging (Phase 8).
///
/// Replaces the parent <c>sp_RecreateSummaries</c> orchestration procedure.
/// </summary>
public sealed class RecreateSummariesOrchestrator
{
    private readonly BatchJobsDbContext _dbContext;
    private readonly IRecreateSummariesStepCatalog _stepCatalog;
    private readonly ILogger<RecreateSummariesOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RecreateSummariesOrchestrator"/>.
    /// </summary>
    public RecreateSummariesOrchestrator(
        BatchJobsDbContext dbContext,
        IRecreateSummariesStepCatalog stepCatalog,
        ILogger<RecreateSummariesOrchestrator> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _stepCatalog = stepCatalog ?? throw new ArgumentNullException(nameof(stepCatalog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes steps 1–14 in order, reads the period-lock flag, and
    /// conditionally executes steps 15–17, all within one transaction.
    /// </summary>
    /// <param name="correlationId">Correlation identifier for this execution.</param>
    /// <param name="month">FPS period month (1–12).</param>
    /// <param name="triggeredBy">Identity of the triggering user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of <see cref="StepResult"/> for every step attempted.</returns>
    public async Task<IReadOnlyList<StepResult>> ExecuteAsync(
        string correlationId,
        int month,
        string triggeredBy,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StepResult>? completedResults = null;
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            var results = new List<StepResult>();

            // Ensure retries start from a clean tracking graph.
            _dbContext.ChangeTracker.Clear();

            var npgsqlConnection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
            if (npgsqlConnection.State != System.Data.ConnectionState.Open)
                await _dbContext.Database.OpenConnectionAsync(cancellationToken);

            var executionContext = new RecreateSummariesExecutionContext(_dbContext, npgsqlConnection);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("[{CorrelationId}] RecreateSummaries implementation: {Implementation}", correlationId, _stepCatalog.ImplementationName);

                // --- Steps 1–14 (mandatory, ordered) ---
                var mandatorySteps = _stepCatalog.BuildMandatorySteps(month, triggeredBy);

                foreach (var step in mandatorySteps)
                {
                    _logger.LogInformation(
                        "[{CorrelationId}] Executing step: {StepName}", correlationId, step.StepName);

                    var result = await step.ExecuteAsync(executionContext, cancellationToken);
                    results.Add(result);

                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
                        correlationId, result.StepName, result.Status, result.RowsAffected,
                        (int)(result.EndTime - result.StartTime).TotalMilliseconds);

                    if (result.Status == Domain.Enums.StepStatus.Failed)
                    {
                        _logger.LogError(
                            "[{CorrelationId}] Step {StepName} failed: {Error}. Rolling back.",
                            correlationId, result.StepName, result.ErrorMessage);

                        await transaction.RollbackAsync(cancellationToken);
                        _dbContext.ChangeTracker.Clear();
                        throw new InvalidOperationException(
                            $"RecreateSummaries step '{result.StepName}' failed: {result.ErrorMessage}");
                    }
                }

                // --- Period-lock check (Phase 6) ---
                var periodLocked = await GetPeriodLockedAsync(month, cancellationToken);

                _logger.LogInformation(
                    "[{CorrelationId}] Period lock check | Month={Month} | PeriodLocked={PeriodLocked}",
                    correlationId, month, periodLocked);

                if (periodLocked == 0)
                {
                    // Steps 15–17: conditional refresh when period is not locked
                    var refreshSteps = _stepCatalog.BuildRefreshSteps(month);

                    foreach (var step in refreshSteps)
                    {
                        _logger.LogInformation(
                            "[{CorrelationId}] Executing refresh step: {StepName}", correlationId, step.StepName);

                        var result = await step.ExecuteAsync(executionContext, cancellationToken);
                        results.Add(result);

                        _logger.LogInformation(
                            "[{CorrelationId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
                            correlationId, result.StepName, result.Status, result.RowsAffected,
                            (int)(result.EndTime - result.StartTime).TotalMilliseconds);

                        if (result.Status == Domain.Enums.StepStatus.Failed)
                        {
                            _logger.LogError(
                                "[{CorrelationId}] Refresh step {StepName} failed: {Error}. Rolling back.",
                                correlationId, result.StepName, result.ErrorMessage);

                            await transaction.RollbackAsync(cancellationToken);
                            _dbContext.ChangeTracker.Clear();
                            throw new InvalidOperationException(
                                $"RecreateSummaries refresh step '{result.StepName}' failed: {result.ErrorMessage}");
                        }
                    }
                }
                else
                {
                    // Period is locked — skip refresh steps, record as Skipped
                    foreach (var stepName in _stepCatalog.BuildRefreshSteps(month).Select(step => step.StepName))
                    {
                        var skipped = new StepResult(stepName, 0, DateTime.UtcNow, DateTime.UtcNow,
                            Domain.Enums.StepStatus.Skipped, "Period is locked");
                        results.Add(skipped);
                        _logger.LogInformation("[{CorrelationId}] Step {StepName} skipped - period is locked.", correlationId, stepName);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                _logger.LogInformation("[{CorrelationId}] Transaction committed. All steps completed.", correlationId);
                completedResults = results;
            }
            catch (Exception) when (results.Count > 0 &&
                                    results[^1].Status != Domain.Enums.StepStatus.Failed)
            {
                // Unexpected exception outside a step failure — attempt rollback
                try { await transaction.RollbackAsync(CancellationToken.None); }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "[{CorrelationId}] Rollback failed.", correlationId);
                }

                // Prevent replay of tracked Added/Modified entities by other repositories sharing this scoped context.
                _dbContext.ChangeTracker.Clear();
                throw;
            }
        });

        return completedResults ?? Array.Empty<StepResult>();
    }

    // -------------------------------------------------------------------------
    // Period-lock helper (Phase 6)
    // -------------------------------------------------------------------------

    private async Task<int> GetPeriodLockedAsync(int month, CancellationToken cancellationToken)
    {
        var periodLocked = await _dbContext.RsTblPeriod
            .AsNoTracking()
            .Where(p => p.EndPeriod == month)
            .Select(p => p.PeriodLocked)
            .FirstOrDefaultAsync(cancellationToken);

        return periodLocked ?? 1;
    }

}
