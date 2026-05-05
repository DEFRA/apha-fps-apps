using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// MABArchive scheduled batch job handler.
/// Loads FPS data from the current and previous calendar years to support financial year reporting
/// into the MABArchive schema within PostgreSQL database.
/// Runs weekly on weekdays at 8:00 PM UTC.
///
/// Lock lifecycle is owned exclusively by JobOrchestrator. This handler must not
/// acquire or release the distributed lock.
/// </summary>
public sealed class MabArchiveJobHandler : IBatchJob
{
    private readonly MabArchiveLoadOrchestrator _orchestrator;
    private readonly IExecutionYearContext _executionYearContext;
    private readonly Func<Func<Task>, Task> _transactionWrapper;
    private readonly ILogger<MabArchiveJobHandler> _logger;
    private readonly ICorrelationService _correlationService;
    private readonly MabArchiveSettings _settings;

    /// <summary>
    /// Canonical job name.
    /// </summary>
    public string Name => "MABArchive";

    /// <summary>
    /// Idempotency strategy: full year-scoped rebuild with deterministic ordering.
    /// </summary>
    public string IdempotencyStrategy => "YearScopedRebuildWithDeterministicOrdering";

    /// <summary>
    /// EventBridge Scheduler cron expression: Monday-Friday at 20:00 (8pm) UTC.
    /// </summary>
    public string? ScheduleExpression => "cron(0 20 ? * MON-FRI *)";

    /// <summary>
    /// Human-readable schedule description.
    /// </summary>
    public string? ScheduleDescription => "Weekdays (Monday to Friday) at 8:00 PM UTC";

    /// <summary>
    /// Maximum execution timeout: 30 minutes.
    /// </summary>
    public int? MaxExecutionSeconds => 1800;

    /// <summary>
    /// Initializes a new instance of the MabArchiveJobHandler.
    /// </summary>
    public MabArchiveJobHandler(
        MabArchiveLoadOrchestrator orchestrator,
        IExecutionYearContext executionYearContext,
        Func<Func<Task>, Task> transactionWrapper,
        ICorrelationService correlationService,
        ILogger<MabArchiveJobHandler> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _executionYearContext = executionYearContext ?? throw new ArgumentNullException(nameof(executionYearContext));
        _transactionWrapper = transactionWrapper ?? throw new ArgumentNullException(nameof(transactionWrapper));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Executes the MABArchive load job.
    /// Lock acquisition and release are handled by JobOrchestrator before this is called.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var runId = $"run-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        var startedAt = DateTime.UtcNow;
        var correlationId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RunId"] = runId,
            ["CorrelationId"] = correlationId,
            ["JobName"] = Name
        });

        _logger.LogInformation("===========================================");
        _logger.LogInformation("MABArchive Job - Starting");
        _logger.LogInformation("===========================================");
        _logger.LogInformation("RunId: {RunId} | Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff} | ProcessId: {ProcessId}",
            runId, startedAt, Environment.ProcessId);

        try
        {
            var context = _orchestrator.BuildExecutionContext();
            _executionYearContext.FpsYear = context.PrimaryYear;
            _executionYearContext.YearSource = "MABArchive.BuildExecutionContext";
            _logger.LogInformation(
                "Execution context built | PrimaryYear={PrimaryYear} | CurrentMonth={CurrentMonth}",
                context.PrimaryYear,
                context.CurrentMonth);

            await _orchestrator.ExecuteAsync(
                runId,
                context,
                _transactionWrapper,
                cancellationToken);

            var duration = DateTime.UtcNow - startedAt;
            _logger.LogInformation("===========================================");
            _logger.LogInformation(
                "MABArchive Job - Completed Successfully | RunId={RunId} | Duration={DurationSeconds}s",
                runId,
                (int)duration.TotalSeconds);
            _logger.LogInformation("===========================================");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "MABArchive job was cancelled | RunId={RunId}", runId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MABArchive job failed with unhandled exception | RunId={RunId}", runId);
            throw;
        }
    }
}
