using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace Apha.BatchJobs.Application.Orchestration;

/// <summary>
/// Implements the full execution lifecycle for a batch job:
/// generate JobQueueId -> acquire lock -> record start -> execute -> record result -> release lock.
/// </summary>
public sealed class JobOrchestrator : IJobOrchestrator
{
    private readonly IBatchJobFactory _factory;
    private readonly IBatchLockRepository _lockRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ICorrelationContextAccessor _correlationService;
    private readonly ICurrentJobExecutionContext _currentExecutionContext;
    private readonly IEmailNotificationService _notificationService;
    private readonly IEnumerable<IPostCompletionNotifier> _postCompletionNotifiers;
    private readonly BatchAlertingSettings _alertingSettings;
    private readonly ILogger<JobOrchestrator> _logger;
    private readonly int _lockTimeoutSeconds;
    private readonly int _retryAttempts;
    private readonly int _retryDelaySeconds;
    private readonly int _maxRetryDurationSeconds;
    private readonly int _defaultJobTimeoutSeconds;
    private readonly int _heartbeatIntervalSeconds;
    private readonly Dictionary<string, int> _jobTimeoutOverridesSeconds;
    private readonly BatchFailureClassifier _failureClassifier;

    /// <summary>Default lock timeout in seconds when configuration is missing/invalid.</summary>
    private const int DefaultLockTimeoutSeconds = 0;

    /// <summary>Default maximum retry duration in seconds.</summary>
    private const int DefaultMaxRetryDurationSeconds = 0;

    public JobOrchestrator(
        IBatchJobFactory factory,
        IBatchLockRepository lockRepository,
        IJobExecutionRepository executionRepository,
        ICorrelationContextAccessor correlationService,
        ICurrentJobExecutionContext currentExecutionContext,
        IEmailNotificationService notificationService,
        IEnumerable<IPostCompletionNotifier> postCompletionNotifiers,
        IOptions<BatchAlertingSettings> alertingSettings,
        IOptions<BatchJobSettings> settings,
        BatchFailureClassifier failureClassifier,
        ILogger<JobOrchestrator> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _lockRepository = lockRepository ?? throw new ArgumentNullException(nameof(lockRepository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _currentExecutionContext = currentExecutionContext ?? throw new ArgumentNullException(nameof(currentExecutionContext));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _postCompletionNotifiers = postCompletionNotifiers ?? [];
        _alertingSettings = alertingSettings?.Value ?? new BatchAlertingSettings();
        _lockTimeoutSeconds = settings?.Value.LockTimeoutSeconds >= 0
            ? settings.Value.LockTimeoutSeconds
            : DefaultLockTimeoutSeconds;
        _retryAttempts = settings?.Value.RetryAttempts >= 0
            ? settings.Value.RetryAttempts
            : 0;
        _retryDelaySeconds = settings?.Value.RetryDelaySeconds >= 0
            ? settings.Value.RetryDelaySeconds
            : 1;
        _maxRetryDurationSeconds = settings?.Value.MaxRetryDurationSeconds >= 0
            ? settings.Value.MaxRetryDurationSeconds
            : DefaultMaxRetryDurationSeconds;
        _defaultJobTimeoutSeconds = settings?.Value.JobTimeout >= 0
            ? settings.Value.JobTimeout
            : DefaultLockTimeoutSeconds;
        _heartbeatIntervalSeconds = settings?.Value.HeartbeatIntervalSeconds > 0
            ? settings.Value.HeartbeatIntervalSeconds
            : 30;
        _jobTimeoutOverridesSeconds = settings?.Value.JobTimeoutOverridesSeconds is { Count: > 0 }
            ? settings.Value.JobTimeoutOverridesSeconds
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value > 0)
                .ToDictionary(kv => kv.Key.Trim(), kv => kv.Value, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failureClassifier = failureClassifier ?? throw new ArgumentNullException(nameof(failureClassifier));
    }

    /// <inheritdoc />
    public async Task<JobExecutionResult> RunAsync(
        string jobName,
        RunMode runMode,
        Guid jobExecutionId,
        string userId,
        DateTime? requestedAtUtc = null,
        string? parametersJson = null,
        CancellationToken cancellationToken = default)
    {
        // Set correlation context so all downstream log events carry the execution ID.
        _correlationService.SetCorrelationId(jobExecutionId.ToString("D"));

        // Resolve optional FPS year from job parameters (used by year-scoped jobs).
        var fpsYear = TryExtractFpsYearFromParameters(parametersJson);

        var startedAt = DateTime.UtcNow;

        // Fetch the Initiated record created by API layer
        var existingExecution = await _executionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId, cancellationToken);

        // Must run before the worker-managed-scheduled fast path below, which otherwise assumes
        // no pre-created row exists and skips this check. Incident precedent: a "MABArchive"
        // request was once allowed to adopt and corrupt a pre-created "YearEnd-DataSetup" row
        // because this validation lived only inside ValidatePreCreatedExecutionRecordAsync,
        // which the fast path bypasses.
        if (existingExecution is not null)
        {
            ValidateExecutionBelongsToRequestedJob(jobName, jobExecutionId, existingExecution);
        }

        var shouldAutoCreateInitiated = IsWorkerManagedScheduledRun(jobName, runMode);
        // Worker-managed MABArchive Scheduled runs may self-create their initiated record below.
        if (!shouldAutoCreateInitiated)
        {
            await ValidatePreCreatedExecutionRecordAsync(jobName, jobExecutionId, existingExecution, fpsYear, cancellationToken);
        }

        if (existingExecution == null && shouldAutoCreateInitiated)
            existingExecution = await AutoCreateInitiatedRecordAsync(jobName, jobExecutionId, userId, requestedAtUtc, startedAt, runMode, fpsYear, cancellationToken);

        if (existingExecution == null)
        {
            _logger.LogError(
                "✗ No Initiated record found for JobExecutionId={JobExecutionId}. Worker cannot proceed without pre-created record from API.",
                jobExecutionId);
            throw new InvalidOperationException(
                $"No Initiated job record found for execution {jobExecutionId}. This indicates the API did not properly create the job record.");
        }

        var jobQueueId = existingExecution.JobQueueId;
        var lockName = ResolveLockName(jobName);
        _logger.LogInformation(
            "[Worker → DB] ✓ Fetched pre-created execution record | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | CurrentStatus={CurrentStatus}",
            jobName, jobExecutionId, jobQueueId, existingExecution.Status);

        _logger.LogInformation(
            "Acquiring execution lock for '{LockName}' (requested job '{JobName}') | JobExecutionId={JobExecutionId}...",
            lockName,
            jobName,
            jobExecutionId);
        var lockAcquired = await _lockRepository.TryAcquireLockAsync(
            lockName,
            jobQueueId,
            _lockTimeoutSeconds,
            cancellationToken);

        if (!lockAcquired)
        {
            _logger.LogError(
                "Job '{JobName}' is already running (lock held by another process). Cannot start execution | JobExecutionId={JobExecutionId}",
                jobName, jobExecutionId);
            throw new InvalidOperationException(
                $"Job '{jobName}' is already running and cannot accept another execution at this time.");
        }
        
        using var runScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobExecutionId"] = jobExecutionId,
            ["JobQueueId"] = jobQueueId,
            ["JobName"] = jobName,
            ["RunMode"] = runMode.ToString(),
            ["UserId"] = userId
        });

        _logger.LogInformation(
            "[Worker → DB] Transitioning from {CurrentStatus} → Running | JobQueueId={JobQueueId}",
            existingExecution.Status, jobQueueId);

        // Step 2 — Create execution record (Started)
        var record = new JobExecutionRecord
        {
            ExecutionId = 0,   // DB assigns real ID on insert
            JobName = jobName,
            JobExecutionId = jobExecutionId,
            JobQueueId = jobQueueId,
            UserId = userId,
            JobType = JobType.Unknown,
            RunMode = runMode,
            Status = JobStatus.Running,
            StartedAt = startedAt,
            RequestedAtUtc = requestedAtUtc,
            FpsYear = fpsYear
        };

        int executionId = 0;
        IDisposable? executionScope = null;
        try
        {
            executionId = await _executionRepository.CreateExecutionRecordAsync(record, cancellationToken);
            record.ExecutionId = executionId;
            if (executionId > 0)
            {
                executionScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["ExecutionId"] = executionId
                });
                _logger.LogInformation("Execution record created | ExecutionId={ExecutionId}", executionId);
            }
            else
            {
                _logger.LogInformation("Execution record created | JobQueueId={JobQueueId}", jobQueueId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not write execution start record — continuing without tracking");
        }

        // Step 3 — Execute the job
        Exception? jobException = null;

        try
        {
            var job = _factory.Create(jobName);

            // Populates the scoped execution context so the job can read its identity and
            // parameters instead of re-parsing env vars — shared by this orchestrator and the
            // job for the lifetime of this one execution.
            _currentExecutionContext.Initialize(
                jobExecutionId, jobQueueId, jobName, runMode, userId, parametersJson, existingExecution.RequestedAtUtc);

            var runtimeTimeoutSeconds = ResolveRuntimeTimeoutSeconds(job);

            _logger.LogInformation(
                "Runtime timeout policy resolved | JobName={JobName} | RuntimeTimeoutSeconds={RuntimeTimeoutSeconds}",
                jobName,
                runtimeTimeoutSeconds?.ToString() ?? "none");

            jobException = await RunJobWithRetryAsync(job, jobName, jobQueueId, runtimeTimeoutSeconds, cancellationToken);
        }
        catch (Exception ex)
        {
            // Captures failures raised before/around ExecuteAsync (e.g. factory resolution) as Failed.
            jobException = ex;
        }
        finally
        {
            executionScope?.Dispose();

            // Step 4 — Update execution record (Completed or Failed)
            var completedAt = DateTime.UtcNow;
            var duration = completedAt - startedAt;
            var finalStatus = jobException switch
            {
                null => JobStatus.Completed,
                OperationCanceledException => JobStatus.Failed,
                _ => JobStatus.Failed
            };

            record.Status = finalStatus;
            record.CompletedAt = completedAt;
            record.DurationSeconds = (int)duration.TotalSeconds;
            record.ErrorMessage = jobException?.Message;
            record.StackTrace = jobException?.StackTrace;

            await MarkFailedSafelyAsync(record, finalStatus, jobException, jobQueueId);

            // Step 5 — Release lock (always), before failure notification runs. Notification is
            // best-effort and must not hold the lock open while it sends.
            try
            {
                await _lockRepository.ReleaseLockAsync(lockName, jobQueueId, CancellationToken.None);
                _logger.LogInformation(
                    "Lock released for '{LockName}' (requested job '{JobName}') | JobQueueId={JobQueueId}",
                    lockName,
                    jobName,
                    jobQueueId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not release lock for '{LockName}' (requested job '{JobName}') | JobQueueId={JobQueueId} — lock will expire after {Timeout}s",
                    lockName,
                    jobName,
                    jobQueueId,
                    _lockTimeoutSeconds);
            }
        }

        var finalDuration = DateTime.UtcNow - startedAt;
        var status = jobException switch
        {
            null => JobStatus.Completed,
            OperationCanceledException => JobStatus.Failed,
            _ => JobStatus.Failed
        };

        _logger.LogInformation(
            "--- Orchestrator: '{JobName}' finished | Status={Status} | Duration={Duration:mm\\:ss\\.fff} | JobQueueId={JobQueueId}",
            jobName, status, finalDuration, jobQueueId);

        if (jobException is null)
        {
            var completionContext = new BatchJobCompletionContext(jobQueueId, jobExecutionId, jobName, fpsYear, userId);
            await TryNotifyCompletionAsync(completionContext, cancellationToken);
        }

        if (jobException is OperationCanceledException cancelEx)
            throw cancelEx;

        if (jobException != null)
        {
            // Runs after the lock has already been released above — best-effort operational
            // reporting is not part of the protected batch execution and must not delay it.
            await TryNotifyFailureAsync(jobName, jobExecutionId, jobException);
            ThrowWithStructuredLog(jobException, jobName, jobQueueId, jobExecutionId);
        }

        return new JobExecutionResult(jobQueueId, jobName, status, finalDuration, executionId);
    }

    private static bool IsWorkerManagedScheduledRun(string jobName, RunMode runMode) =>
        runMode == RunMode.Scheduled &&
        (string.Equals(jobName, BatchJobNames.MabArchive, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(jobName, BatchJobNames.MilestoneUpdateNotifications, StringComparison.OrdinalIgnoreCase));

    private async Task<JobExecutionRecord> AutoCreateInitiatedRecordAsync(
        string jobName, Guid jobExecutionId, string userId,
        DateTime? requestedAtUtc, DateTime startedAt, RunMode runMode, int? fpsYear,
        CancellationToken cancellationToken)
    {
        var initiatedRequestedAtUtc = requestedAtUtc ?? startedAt;
        try
        {
            var createdJobQueueId = await _executionRepository.CreateInitiatedRecordAsync(
                jobName, jobExecutionId, userId, initiatedRequestedAtUtc, runMode, cancellationToken, fpsYear);

            _logger.LogWarning(
                "Initiated record was missing for scheduled worker-managed run. Created worker-managed Initiated row in worker | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | RunMode={RunMode}",
                jobName, jobExecutionId, createdJobQueueId, runMode);

            return new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = jobName,
                JobExecutionId = jobExecutionId,
                JobQueueId = createdJobQueueId,
                UserId = userId,
                JobType = JobType.Unknown,
                RunMode = runMode,
                Status = JobStatus.Initiated,
                RequestedAtUtc = initiatedRequestedAtUtc,
                FpsYear = fpsYear,
                StartedAt = initiatedRequestedAtUtc
            };
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var existing = await _executionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId, cancellationToken);
            if (existing == null) throw;

            _logger.LogInformation(
                "Initiated record appeared concurrently while creating worker-managed row. Proceeding with existing row | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | RunMode={RunMode}",
                jobName, jobExecutionId, existing.JobQueueId, runMode);
            return existing;
        }
    }

    private async Task<Exception?> RunJobWithRetryAsync(
        IBatchJob job, string jobName, Guid jobQueueId,
        int? runtimeTimeoutSeconds, CancellationToken cancellationToken)
    {
        var retryStartedAt = DateTime.UtcNow;
        var totalAttempts = _retryAttempts + 1;

        for (var attempt = 1; attempt <= totalAttempts; attempt++)
        {
            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (runtimeTimeoutSeconds.HasValue)
                attemptCts.CancelAfter(TimeSpan.FromSeconds(runtimeTimeoutSeconds.Value));

            var attemptToken = attemptCts.Token;

            // Heartbeat now happens at job/step entry points, not a background loop.
            _logger.LogInformation(
                "Heartbeat strategy: step-based checks (not background loop) | JobName={JobName} | JobQueueId={JobQueueId}",
                jobName, jobQueueId);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation(
                    "Executing job '{JobName}' | Attempt={Attempt}/{TotalAttempts}",
                    jobName, attempt, totalAttempts);
                await job.ExecuteAsync(attemptToken);
                _logger.LogInformation(
                    "Job '{JobName}' completed successfully | Attempt={Attempt}/{TotalAttempts}",
                    jobName, attempt, totalAttempts);
                return null;
            }
            catch (OperationCanceledException ex)
            {
                if (attemptCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    var timeoutEx = new TimeoutException(
                        $"Job '{jobName}' exceeded runtime timeout of {runtimeTimeoutSeconds} seconds.", ex);
                    _logger.LogError(timeoutEx,
                        "Job '{JobName}' exceeded runtime timeout and was stopped | Attempt={Attempt}/{TotalAttempts} | RuntimeTimeoutSeconds={RuntimeTimeoutSeconds}",
                        jobName, attempt, totalAttempts, runtimeTimeoutSeconds);
                    return timeoutEx;
                }
                _logger.LogWarning(ex,
                    "Job '{JobName}' execution was interrupted by cancellation token | Attempt={Attempt}/{TotalAttempts}",
                    jobName, attempt, totalAttempts);
                return ex;
            }
            catch (Exception ex)
            {
                var maybeRetry = await HandleRetryableExceptionAsync(
                    ex, jobName, jobQueueId, attempt, totalAttempts, retryStartedAt, cancellationToken);
                if (maybeRetry != null)
                    return maybeRetry;
            }
            finally
            {
                // No background heartbeat task to cancel (step-based strategy in place)
                attemptCts.Cancel();
            }
        }

        return null;
    }

    // Returns the exception to surface (stop retrying), or null to continue with the next attempt.
    private async Task<Exception?> HandleRetryableExceptionAsync(
        Exception ex, string jobName, Guid jobQueueId,
        int attempt, int totalAttempts, DateTime retryStartedAt, CancellationToken cancellationToken)
    {
        var isRetryable = IsRetryable(ex);
        var exceptionClassification = isRetryable ? "TransientRetryable" : "NonRetryable";
        _logger.LogInformation(
            "Job exception classification | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | Classification={ExceptionClassification}",
            attempt, totalAttempts, ex.GetType().Name, exceptionClassification);

        if (!isRetryable)
        {
            _logger.LogError(ex,
                "Job '{JobName}' failed with non-retryable exception | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | ErrorMessage={ErrorMessage} | JobQueueId={JobQueueId}",
                jobName, attempt, totalAttempts, ex.GetType().Name, ex.Message, jobQueueId);
            return ex;
        }

        if (attempt >= totalAttempts)
        {
            _logger.LogError(ex,
                "Job '{JobName}' failed after retries exhausted | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | ErrorMessage={ErrorMessage} | JobQueueId={JobQueueId}",
                jobName, attempt, totalAttempts, ex.GetType().Name, ex.Message, jobQueueId);
            return ex;
        }

        var elapsedRetrySeconds = (DateTime.UtcNow - retryStartedAt).TotalSeconds;
        if (_maxRetryDurationSeconds > 0 && elapsedRetrySeconds >= _maxRetryDurationSeconds)
        {
            _logger.LogError(ex,
                "Job '{JobName}' retry duration capped | Attempt={Attempt}/{TotalAttempts} | ElapsedRetrySeconds={ElapsedSeconds} | MaxRetrySeconds={MaxSeconds} | JobQueueId={JobQueueId}",
                jobName, attempt, totalAttempts, (int)elapsedRetrySeconds, _maxRetryDurationSeconds, jobQueueId);
            return ex;
        }

        var basedelaySeconds = _retryDelaySeconds;
        var jitterSeconds = new Random().Next(0, Math.Max(1, basedelaySeconds / 2));
        var finalDelaySeconds = basedelaySeconds + jitterSeconds;

        _logger.LogWarning(ex,
            "Job '{JobName}' failed | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | Classification={ExceptionClassification} | Retrying after {RetryDelaySeconds}s (+{JitterSeconds}s jitter) | JobQueueId={JobQueueId}",
            jobName, attempt, totalAttempts, ex.GetType().Name, exceptionClassification,
            basedelaySeconds, jitterSeconds, jobQueueId);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(finalDelaySeconds), cancellationToken);
        }
        catch (OperationCanceledException cancelDelayEx)
        {
            _logger.LogWarning(cancelDelayEx,
                "Retry delay cancelled for '{JobName}' | Attempt={Attempt}/{TotalAttempts}",
                jobName, attempt, totalAttempts);
            return cancelDelayEx;
        }

        return null; // proceed with next attempt
    }

    /// <summary>
    /// Persists the final execution status. If that write itself fails, logs at Critical (so
    /// CloudWatch alarms still fire) and swallows the error so the container still exits with
    /// the original job's exit code.
    /// </summary>
    private async Task MarkFailedSafelyAsync(
        JobExecutionRecord record,
        JobStatus finalStatus,
        Exception? originalException,
        Guid jobQueueId)
    {
        try
        {
            await _executionRepository.UpdateExecutionRecordAsync(record, CancellationToken.None);
            _logger.LogInformation(
                "Execution record updated | Status={Status} | Duration={DurationSeconds}s",
                finalStatus, record.DurationSeconds);
        }
        catch (Exception ex)
        {
            var originalType = originalException?.GetType().Name ?? "None";
            var sqlType = _failureClassifier.Classify(ex).ErrorType;
            _logger.LogCritical(ex,
                "[{ErrorType}] Could not write execution completion record — job result may not be persisted | OriginalExceptionType={OriginalExceptionType} | JobQueueId={JobQueueId}",
                sqlType, originalType, jobQueueId);
        }
    }

    /// <summary>
    /// Logs a structured error with the correct <c>[{ErrorType}]</c> token for the given exception type,
    /// then re-throws the exception preserving the original stack trace.
    /// </summary>
    private void ThrowWithStructuredLog(Exception exception, string jobName, Guid jobQueueId, Guid jobExecutionId)
    {
        // Only Sql and General have CloudWatch alarms wired up; Configuration and email
        // failures roll up into General rather than an unwatched alarm channel.
        var errorType = _failureClassifier.Classify(exception).ErrorType;

        _logger.LogError(
            exception,
            "[{ErrorType}] Batch job failed | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId}",
            errorType,
            jobName,
            jobQueueId,
            jobExecutionId);

        ExceptionDispatchInfo.Capture(exception).Throw();
    }

    /// <summary>
    /// Invokes all registered post-completion notifiers after a job is durably Completed and
    /// its lock released. Failures are logged and swallowed — a notification problem must not
    /// alter the durable Completed state.
    /// </summary>
    private async Task TryNotifyCompletionAsync(BatchJobCompletionContext context, CancellationToken cancellationToken)
    {
        foreach (var notifier in _postCompletionNotifiers)
        {
            try
            {
                await notifier.NotifyAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Post-completion notifier {NotifierType} failed | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId}",
                    notifier.GetType().Name,
                    context.JobName,
                    context.JobQueueId,
                    context.JobExecutionId);
            }
        }
    }

    /// <summary>
    /// Sends a best-effort failure notification once retries are exhausted. Never lets a
    /// notification failure mask the original job exception. Gated on
    /// <see cref="BatchAlertingSettings.EnableEmailNotifications"/> and job-name membership in
    /// <see cref="BatchAlertingSettings.EmailEnabledJobs"/>.
    /// </summary>
    private async Task TryNotifyFailureAsync(string jobName, Guid jobExecutionId, Exception exception)
    {
        if (!_alertingSettings.EnableEmailNotifications ||
            !_alertingSettings.EmailEnabledJobs.Contains(jobName, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            await _notificationService.SendFailureNotificationAsync(
                jobExecutionId.ToString("D"),
                jobName,
                exception.Message,
                DateTime.UtcNow,
                CancellationToken.None);
        }
        catch (Exception notifyEx)
        {
            _logger.LogWarning(
                notifyEx,
                "Failed to send failure notification | JobName={JobName} | JobExecutionId={JobExecutionId}",
                jobName,
                jobExecutionId);
        }
    }

    private static string ResolveLockName(string jobName)
    {
        return jobName switch
        {
            BatchJobNames.YearEndDataSetup => BatchJobNames.YearEndLock,
            BatchJobNames.YearEndCutover => BatchJobNames.YearEndLock,
            _ => jobName
        };
    }

    /// <summary>
    /// True only for explicit transient infrastructure failures (timeouts, connectivity).
    /// Configuration, validation, and business-rule errors are never retried.
    /// </summary>
    public static bool IsRetryable(Exception ex) => ex switch
    {
        OperationCanceledException => false,
        ArgumentException => false,
        InvalidOperationException => false,
        NotSupportedException => false,
        NotImplementedException => false,

        TimeoutException => true,
        NpgsqlException => true,
        DbUpdateException => true,
        HttpRequestException => true,
        System.Net.Sockets.SocketException => true,
        IOException => true,

        _ => false
    };

    private int? ResolveRuntimeTimeoutSeconds(IBatchJob job)
    {
        if (_jobTimeoutOverridesSeconds.TryGetValue(job.Name, out var overrideSeconds) && overrideSeconds > 0)
        {
            return overrideSeconds;
        }

        if (job.MaxExecutionSeconds.HasValue && job.MaxExecutionSeconds.Value > 0)
        {
            return job.MaxExecutionSeconds.Value;
        }

        return _defaultJobTimeoutSeconds > 0 ? _defaultJobTimeoutSeconds : null;
    }

    private async Task RunHeartbeatLoopAsync(string jobName, Guid jobQueueId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_heartbeatIntervalSeconds), cancellationToken);

                var heartbeatWritten = await _executionRepository.TouchRunningExecutionAsync(jobQueueId, cancellationToken);
                if (!heartbeatWritten)
                {
                    return;
                }

                var lockRenewed = await _lockRepository.TryRenewLockAsync(
                    jobName,
                    jobQueueId,
                    _lockTimeoutSeconds,
                    cancellationToken);

                if (!lockRenewed)
                {
                    _logger.LogWarning(
                        "Heartbeat wrote execution metadata but lock renewal failed | JobName={JobName} | JobQueueId={JobQueueId}",
                        jobName,
                        jobQueueId);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Heartbeat loop iteration failed | JobName={JobName} | JobQueueId={JobQueueId}",
                    jobName,
                    jobQueueId);
            }
        }
    }

    // ─── Execution contract validation ──────────────────────────────────────────

    /// <summary>
    /// A row found by jobExecutionId alone is not proof it belongs to the requested job — a
    /// reused execution ID for a different job must never be silently adopted. Called
    /// unconditionally in <see cref="RunAsync"/> before any other branching, so nothing can
    /// bypass it.
    /// </summary>
    private static void ValidateExecutionBelongsToRequestedJob(string jobName, Guid jobExecutionId, JobExecutionRecord existingExecution)
    {
        if (!string.Equals(existingExecution.JobName, jobName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' already belongs to job '{existingExecution.JobName}', not '{jobName}'.");
        }
    }

    private async Task ValidatePreCreatedExecutionRecordAsync(
        string jobName,
        Guid jobExecutionId,
        JobExecutionRecord? existingExecution,
        int? targetFpsYear,
        CancellationToken cancellationToken)
    {
        var expectedPickupStatus = IsApprovalBasedJob(jobName) ? JobStatus.Approved : JobStatus.Initiated;

        if (existingExecution is null)
        {
            throw new InvalidOperationException(
                $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' has no pre-created {expectedPickupStatus} row. " +
                $"API must insert and prepare {expectedPickupStatus} before worker start.");
        }

        // Job identity is already validated in RunAsync before this method runs — not repeated here.

        _logger.LogInformation(
            "Found existing execution record | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | Status={Status}",
            jobExecutionId, existingExecution.JobQueueId, existingExecution.Status);

        if (targetFpsYear.HasValue && existingExecution.TargetFpsYear.HasValue && existingExecution.TargetFpsYear.Value != targetFpsYear.Value)
        {
            throw new InvalidOperationException(
                $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' has target_fpsyear '{existingExecution.TargetFpsYear.Value}' " +
                $"but trigger requested targetFpsYear '{targetFpsYear.Value}'.");
        }

        if (existingExecution.Status == expectedPickupStatus)
        {
            _logger.LogInformation(
                "Execution contract pre-check passed | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | JobName={JobName} | ExpectedPickupStatus={ExpectedPickupStatus}",
                jobExecutionId, existingExecution.JobQueueId, jobName, expectedPickupStatus);

            if (IsApprovalBasedJob(jobName))
                await ValidateApprovalMetadataAsync(jobName, jobExecutionId, cancellationToken);

            return;
        }

        if (existingExecution.Status == JobStatus.Running)
        {
            throw new InvalidOperationException(
                $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' is already running (JobQueueId={existingExecution.JobQueueId}). Parallel execution not permitted.");
        }

        var isTerminal = existingExecution.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Rejected;
        if (isTerminal)
        {
            throw new InvalidOperationException(
                $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' was already used by terminal execution '{existingExecution.Status}' (JobQueueId={existingExecution.JobQueueId}). Replays are not permitted.");
        }

        throw new InvalidOperationException(
            $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' is in an unexpected status '{existingExecution.Status}' " +
            $"(JobQueueId={existingExecution.JobQueueId}). Expected pickup status: '{expectedPickupStatus}'.");
    }

    private async Task ValidateApprovalMetadataAsync(string jobName, Guid jobExecutionId, CancellationToken cancellationToken)
    {
        var metadata = await _executionRepository.GetApprovalMetadataAsync(jobExecutionId, cancellationToken);

        if (metadata is null || string.IsNullOrWhiteSpace(metadata.ApprovedBy) || !metadata.ApprovedAtUtc.HasValue)
        {
            throw new InvalidOperationException(
                $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' for job '{jobName}' is Approved " +
                "but is missing approval metadata (approved_by/approved_at_utc) in fps.job_queue.");
        }

        _logger.LogInformation(
            "Approval metadata verified | JobName={JobName} | JobExecutionId={JobExecutionId} | ApprovedBy={ApprovedBy} | ApprovedAtUtc={ApprovedAtUtc}",
            jobName, jobExecutionId, metadata.ApprovedBy, metadata.ApprovedAtUtc);
    }

    private static bool IsApprovalBasedJob(string jobName) =>
        IsYearEndJob(jobName)
        || string.Equals(jobName, BatchJobNames.BulkTestRatesUpdate, StringComparison.OrdinalIgnoreCase)
        || string.Equals(jobName, BatchJobNames.BulkStaffRatesUpdate, StringComparison.OrdinalIgnoreCase)
        || string.Equals(jobName, BatchJobNames.BulkAnimalRatesUpdate, StringComparison.OrdinalIgnoreCase);

    private static bool IsYearEndJob(string jobName) =>
        string.Equals(jobName, BatchJobNames.YearEndDataSetup, StringComparison.OrdinalIgnoreCase)
        || string.Equals(jobName, BatchJobNames.YearEndCutover, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Extracts the requested FPS year from job parameters. Reads <c>plannedYear</c> (what
    /// production sends), falling back to the legacy <c>targetFpsYear</c> alias. Throws if both
    /// are present and disagree, rather than silently picking one.
    /// </summary>
    private static int? TryExtractFpsYearFromParameters(string? parametersJson)
    {
        var plannedYear = TryExtractIntField(parametersJson, "plannedYear");
        var legacyTargetFpsYear = TryExtractIntField(parametersJson, "targetFpsYear");

        if (plannedYear.HasValue && legacyTargetFpsYear.HasValue && plannedYear.Value != legacyTargetFpsYear.Value)
        {
            throw new InvalidOperationException(
                $"Job parameters contain conflicting year values: plannedYear={plannedYear.Value}, " +
                $"targetFpsYear={legacyTargetFpsYear.Value}. These must agree or only one should be supplied.");
        }

        return plannedYear ?? legacyTargetFpsYear;
    }

    private static int? TryExtractIntField(string? parametersJson, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(parametersJson);
            // EventBridge passes "null" when parametersJson is absent; treat non-object root as absent.
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;
            if (!doc.RootElement.TryGetProperty(propertyName, out var el))
                return null;

            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var num))
                return num;

            if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out var str))
                return str;
        }
        catch (JsonException)
        {
            // Malformed JSON — year is not extractable; callers treat null as absent.
        }

        return null;
    }
}
