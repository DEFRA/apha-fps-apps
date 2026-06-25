using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

/// <summary>
/// RecreateSummaries scheduled batch job handler.
/// Rebuilds monthly FPS summary/calculation data by executing 14 ordered SQL steps
/// and optionally refreshing period snapshot tables when the period is unlocked.
///
/// Replaces the legacy SQL Server <c>sp_RecreateSummaries</c> orchestration procedure.
///
/// Lock lifecycle is owned exclusively by <see cref="JobOrchestrator"/>.
/// This handler must not acquire or release the distributed lock.
/// </summary>

public sealed class RecreateSummariesJobHandler : IBatchJob
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IRecreateSummariesStepCatalog _stepCatalog;
    private readonly IRecreateSummariesContext _jobContext;
    private readonly IJobExecutionRepository? _executionRepository;
    private readonly IBatchLockRepository? _lockRepository;
    private readonly ICorrelationService _correlationService;
    private readonly ILogger<RecreateSummariesJobHandler> _logger;

    /// <summary>Canonical job name.</summary>
    public string Name => "RecreateSummary";

    /// <summary>
    /// Idempotency strategy: full delete-and-rebuild per month with a single wrapping transaction.
    /// </summary>
    public string IdempotencyStrategy => "DeleteAndRebuildWithSingleTransaction";

    /// <summary>
    /// RecreateSummaries is a manually triggered job — no schedule expression.
    /// </summary>
    public string? ScheduleExpression => null;

    /// <summary>Human-readable schedule description.</summary>
    public string? ScheduleDescription => "Manually triggered per FPS period month";

    /// <summary>Maximum execution timeout: 60 minutes.</summary>
    public int? MaxExecutionSeconds => 3600;

    /// <summary>
    /// Initializes a new instance of <see cref="RecreateSummariesJobHandler"/>.
    /// </summary>
    public RecreateSummariesJobHandler(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IRecreateSummariesStepCatalog stepCatalog,
        IRecreateSummariesContext jobContext,
        IJobExecutionRepository? executionRepository,
        IBatchLockRepository? lockRepository,
        ICorrelationService correlationService,
        ILogger<RecreateSummariesJobHandler> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _stepCatalog = stepCatalog ?? throw new ArgumentNullException(nameof(stepCatalog));
        _jobContext = jobContext ?? throw new ArgumentNullException(nameof(jobContext));
        _executionRepository = executionRepository;
        _lockRepository = lockRepository;
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var correlationId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["JobName"] = Name,
            ["Month"] = _jobContext.Month,
            ["Year"] = _jobContext.Year,
            ["TriggeredBy"] = _jobContext.TriggeredBy
        });

        _logger.LogInformation("===========================================");
        _logger.LogInformation("RecreateSummaries Job - Starting");
        _logger.LogInformation("===========================================");
        _logger.LogInformation(
            "CorrelationId: {CorrelationId} | Month: {Month} | Year: {Year} | TriggeredBy: {TriggeredBy} | Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff}",
            correlationId, _jobContext.Month, _jobContext.Year, _jobContext.TriggeredBy, startedAt);

        try
        {
            var results = await ExecuteStepsAsync(
                correlationId,
                _jobContext.Month,
                _jobContext.Year,
                _jobContext.TriggeredBy,
                cancellationToken);

            var duration = DateTime.UtcNow - startedAt;

            _logger.LogInformation("===========================================");
            _logger.LogInformation(
                "RecreateSummaries Job - Completed Successfully | CorrelationId={CorrelationId} | Month={Month} | Year={Year} | Steps={StepCount} | Duration={DurationSeconds}s",
                correlationId, _jobContext.Month, _jobContext.Year, results.Count, (int)duration.TotalSeconds);
            _logger.LogInformation("===========================================");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "RecreateSummaries job execution was interrupted | CorrelationId={CorrelationId}", correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RecreateSummaries job failed | CorrelationId={CorrelationId} | Month={Month} | Year={Year}", correlationId, _jobContext.Month, _jobContext.Year);
            throw;
        }
    }

    /// <summary>
    /// Executes steps 1–14 in order, reads the period-lock flag, and
    /// conditionally executes steps 15–17, all within one transaction.
    /// Includes step-based heartbeat checks and slow-step detection.
    /// </summary>
    private async Task<IReadOnlyList<StepResult>> ExecuteStepsAsync(
        string correlationId,
        int month,
        int year,
        string triggeredBy,
        CancellationToken cancellationToken)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        IReadOnlyList<StepResult>? completedResults = null;
        var executionStrategy = context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            var results = new List<StepResult>();

            // Ensure retries start from a clean tracking graph.
            context.ChangeTracker.Clear();

            var npgsqlConnection = (NpgsqlConnection)context.Database.GetDbConnection();
            if (npgsqlConnection.State != System.Data.ConnectionState.Open)
                await context.Database.OpenConnectionAsync(cancellationToken);

            var executionContext = new RecreateSummariesExecutionContext(context, npgsqlConnection);

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("[{CorrelationId}] RecreateSummaries implementation: DotNetLinq", correlationId);

                // --- Steps 1–14 (mandatory, ordered) ---
                var mandatorySteps = _stepCatalog.BuildMandatorySteps(month, year, triggeredBy);

                foreach (var step in mandatorySteps)
                {
                    // Step-based heartbeat: touch execution and renew lock before starting step
                    await TouchHeartbeatAsync(correlationId, cancellationToken);

                    _logger.LogInformation(
                        "[{CorrelationId}] Executing step: {StepName}", correlationId, step.StepName);

                    var result = await step.ExecuteAsync(executionContext, cancellationToken);
                    results.Add(result);

                    var stepDurationMs = (int)(result.EndTime - result.StartTime).TotalMilliseconds;
                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
                        correlationId, result.StepName, result.Status, result.RowsAffected,
                        stepDurationMs);

                    // Warn if step exceeded 2 minutes (slow-step detection)
                    if (stepDurationMs > 120_000)
                    {
                        _logger.LogWarning(
                            "[{CorrelationId}] SLOW STEP DETECTED | StepName={StepName} | Duration={Ms}ms | RowsAffected={Rows}",
                            correlationId, result.StepName, stepDurationMs, result.RowsAffected);
                    }

                    if (result.Status == Domain.Enums.StepStatus.Failed)
                    {
                        _logger.LogError(
                            "[{CorrelationId}] Step {StepName} failed: {Error}. Rolling back.",
                            correlationId, result.StepName, result.ErrorMessage);

                        await SafeRollbackAsync(transaction, correlationId);
                        context.ChangeTracker.Clear();
                        throw new InvalidOperationException(
                            $"RecreateSummaries step '{result.StepName}' failed: {result.ErrorMessage}");
                    }
                }

                // --- Period-lock check (Phase 6) ---
                var periodLocked = await GetPeriodLockedAsync(context, month, year, cancellationToken);

                _logger.LogInformation(
                    "[{CorrelationId}] Period lock check | Month={Month} | Year={Year} | PeriodLocked={PeriodLocked}",
                    correlationId, month, year, periodLocked);

                if (periodLocked == 0)
                {
                    // Steps 15–17: conditional refresh when period is not locked
                    var refreshSteps = _stepCatalog.BuildRefreshSteps(month);

                    foreach (var step in refreshSteps)
                    {
                        // Step-based heartbeat: touch execution and renew lock before starting step
                        await TouchHeartbeatAsync(correlationId, cancellationToken);

                        _logger.LogInformation(
                            "[{CorrelationId}] Executing refresh step: {StepName}", correlationId, step.StepName);

                        var result = await step.ExecuteAsync(executionContext, cancellationToken);
                        results.Add(result);

                        var stepDurationMs = (int)(result.EndTime - result.StartTime).TotalMilliseconds;
                        _logger.LogInformation(
                            "[{CorrelationId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
                            correlationId, result.StepName, result.Status, result.RowsAffected,
                            stepDurationMs);

                        // Warn if step exceeded 2 minutes (slow-step detection)
                        if (stepDurationMs > 120_000)
                        {
                            _logger.LogWarning(
                                "[{CorrelationId}] SLOW STEP DETECTED | StepName={StepName} | Duration={Ms}ms | RowsAffected={Rows}",
                                correlationId, result.StepName, stepDurationMs, result.RowsAffected);
                        }

                        if (result.Status == Domain.Enums.StepStatus.Failed)
                        {
                            _logger.LogError(
                                "[{CorrelationId}] Refresh step {StepName} failed: {Error}. Rolling back.",
                                correlationId, result.StepName, result.ErrorMessage);

                            await SafeRollbackAsync(transaction, correlationId);
                            context.ChangeTracker.Clear();
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
                            StepStatus.Skipped, "Period is locked");
                        results.Add(skipped);
                        _logger.LogInformation("[{CorrelationId}] Step {StepName} skipped - period is locked.", correlationId, stepName);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                context.ChangeTracker.Clear();

                _logger.LogInformation("[{CorrelationId}] Transaction committed. All steps completed.", correlationId);
                completedResults = results;
            }
            catch (Exception) when (results.Count > 0 &&
                                    results[^1].Status != Domain.Enums.StepStatus.Failed)
            {
                // Unexpected exception outside a step failure — attempt rollback
                await SafeRollbackAsync(transaction, correlationId);

                // Prevent replay of tracked Added/Modified entities by other repositories sharing this scoped context.
                context.ChangeTracker.Clear();
                throw;
            }
        });

        return completedResults ?? Array.Empty<StepResult>();
    }

    /// <summary>
    /// Step-based heartbeat: touches execution metadata and renews lock before each step.
    /// Logs success/failure to correlate with step execution for timeout debugging.
    /// </summary>
    private async Task TouchHeartbeatAsync(string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            if (_executionRepository is not null && Guid.TryParse(correlationId, out var jobExecutionId))
            {
                var touched = await _executionRepository.TouchRunningExecutionAsync(
                    jobExecutionId, cancellationToken);

                if (!touched)
                {
                    _logger.LogWarning(
                        "[{CorrelationId}] Heartbeat: execution touch failed (not in Running state) | JobName={JobName}",
                        correlationId, Name);
                    return;
                }
            }

            if (_lockRepository is not null)
            {
                // Unlimited timeout (0 seconds) means lock does not expire
                var renewed = await _lockRepository.TryRenewLockAsync(
                    Name, Guid.Empty, 0, cancellationToken);

                if (!renewed)
                {
                    _logger.LogWarning(
                        "[{CorrelationId}] Heartbeat: lock renewal returned false | JobName={JobName}",
                        correlationId, Name);
                }
                else
                {
                    _logger.LogDebug(
                        "[{CorrelationId}] Heartbeat: execution touched & lock renewed | JobName={JobName}",
                        correlationId, Name);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation during heartbeat is expected during shutdown
            _logger.LogInformation(
                "[{CorrelationId}] Heartbeat cancelled | JobName={JobName}",
                correlationId, Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[{CorrelationId}] Heartbeat exception (non-critical) | JobName={JobName} | ExceptionType={ExceptionType}",
                correlationId, Name, ex.GetType().Name);
        }
    }

    private async Task<int> GetPeriodLockedAsync(
        BatchJobsDbContext context,
        int month,
        int year,
        CancellationToken cancellationToken)
    {
        var periodLocked = await context.RsTblPeriod
            .AsNoTracking()
            .Where(p => p.EndPeriod == month && p.FpsYear == year)
            .Select(p => p.PeriodLocked)
            .FirstOrDefaultAsync(cancellationToken);

        return periodLocked ?? 1;
    }

    private async Task SafeRollbackAsync(IDbContextTransaction transaction, string correlationId)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, "[{CorrelationId}] Rollback failed.", correlationId);
        }
    }
}