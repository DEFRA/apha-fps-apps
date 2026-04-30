using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// MABArchive scheduled batch job handler.
/// Loads FPS data from the current and previous calendar years to support financial year reporting
/// into the MABArchive schema within PostgreSQL database.
/// Runs weekly on weekdays at 8:00 PM UTC.
/// </summary>
public sealed class MabArchiveJobHandler : IBatchJob
{
    private readonly MabArchiveLoadOrchestrator _orchestrator;
    private readonly IBatchLockRepository _lockRepository;
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
        IBatchLockRepository lockRepository,
        Func<Func<Task>, Task> transactionWrapper,
        ICorrelationService correlationService,
        ILogger<MabArchiveJobHandler> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _lockRepository = lockRepository ?? throw new ArgumentNullException(nameof(lockRepository));
        _transactionWrapper = transactionWrapper ?? throw new ArgumentNullException(nameof(transactionWrapper));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Executes the MABArchive load job.
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
            // Attempt to acquire lock
            var lockAcquired = await _lockRepository.TryAcquireLockAsync(
                Name,
                runId,
                _settings.LockTimeoutSeconds,
                cancellationToken);

            if (!lockAcquired)
            {
                _logger.LogWarning(
                    "MABArchive job is already running (lock held by another process). Skipping this run.");
                return;
            }

            _logger.LogInformation("Lock acquired for MABArchive job | RunId={RunId}", runId);

            try
            {
                // Build execution context
                var context = _orchestrator.BuildExecutionContext();
                _logger.LogInformation(
                    "Execution context built | PrimaryYear={PrimaryYear} | CurrentMonth={CurrentMonth}",
                    context.PrimaryYear,
                    context.CurrentMonth);

                // Execute orchestration within transaction
                await _orchestrator.ExecuteAsync(
                    runId,
                    context,
                    _transactionWrapper,
                    cancellationToken);

                var duration = DateTime.UtcNow - startedAt;
                _logger.LogInformation(
                    "===========================================");
                _logger.LogInformation(
                    "MABArchive Job - Completed Successfully | RunId={RunId} | Duration={DurationSeconds}s",
                    runId,
                    (int)duration.TotalSeconds);
                _logger.LogInformation("===========================================");
            }
            finally
            {
                // Release lock
                try
                {
                    await _lockRepository.ReleaseLockAsync(Name, runId, CancellationToken.None);
                    _logger.LogInformation("Lock released for MABArchive job | RunId={RunId}", runId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release lock for MABArchive job | RunId={RunId}", runId);
                }
            }
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
