using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private readonly RecreateSummariesOrchestrator _orchestrator;
    private readonly IRecreateSummariesContext _jobContext;
    private readonly ICorrelationService _correlationService;
    private readonly ILogger<RecreateSummariesJobHandler> _logger;

    /// <summary>Canonical job name.</summary>
    public string Name => "RecreateSummaries";

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
        RecreateSummariesOrchestrator orchestrator,
        IRecreateSummariesContext jobContext,
        ICorrelationService correlationService,
        ILogger<RecreateSummariesJobHandler> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _jobContext = jobContext ?? throw new ArgumentNullException(nameof(jobContext));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var runId = $"run-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
        var startedAt = DateTime.UtcNow;
        var correlationId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RunId"] = runId,
            ["CorrelationId"] = correlationId,
            ["JobName"] = Name,
            ["Month"] = _jobContext.Month,
            ["TriggeredBy"] = _jobContext.TriggeredBy
        });

        _logger.LogInformation("===========================================");
        _logger.LogInformation("RecreateSummaries Job - Starting");
        _logger.LogInformation("===========================================");
        _logger.LogInformation(
            "RunId: {RunId} | Month: {Month} | TriggeredBy: {TriggeredBy} | Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff}",
            runId, _jobContext.Month, _jobContext.TriggeredBy, startedAt);

        try
        {
            var results = await _orchestrator.ExecuteAsync(
                runId,
                _jobContext.Month,
                _jobContext.TriggeredBy,
                cancellationToken);

            var duration = DateTime.UtcNow - startedAt;

            _logger.LogInformation("===========================================");
            _logger.LogInformation(
                "RecreateSummaries Job - Completed Successfully | RunId={RunId} | Month={Month} | Steps={StepCount} | Duration={DurationSeconds}s",
                runId, _jobContext.Month, results.Count, (int)duration.TotalSeconds);
            _logger.LogInformation("===========================================");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "RecreateSummaries job was cancelled | RunId={RunId}", runId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RecreateSummaries job failed | RunId={RunId} | Month={Month}", runId, _jobContext.Month);
            throw;
        }
    }
}