using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Entities.Email;
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
    private readonly IBatchLockReconciliationService _reconciliationService;
    private readonly IHeartbeatRepositoryScopeFactory _heartbeatRepositoryScopeFactory;
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
        IBatchLockReconciliationService reconciliationService,
        IHeartbeatRepositoryScopeFactory heartbeatRepositoryScopeFactory,
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
        _reconciliationService = reconciliationService ?? throw new ArgumentNullException(nameof(reconciliationService));
        _heartbeatRepositoryScopeFactory = heartbeatRepositoryScopeFactory ?? throw new ArgumentNullException(nameof(heartbeatRepositoryScopeFactory));
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

        // Layer 2 — scoped, defensive reconciliation immediately before acquisition. Only the
        // resolved lock name this execution actually needs is inspected — never a system-wide
        // scan (design doc §4/§9). The reconciliation service itself decides whether this row is
        // actually expired (it refuses as a no-op otherwise), so nothing here needs to duplicate
        // that check. Deliberately not wrapped in try/catch: an unexpected failure here must
        // propagate and prevent acquisition rather than be logged-and-ignored — lock state that
        // can't be trusted must not be treated as safe to acquire against.
        var existingLock = await _lockRepository.GetLockAsync(lockName, cancellationToken);
        if (existingLock is not null)
        {
            await _reconciliationService.ReconcileAsync(existingLock, cancellationToken);
        }

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
            FpsYear = fpsYear,
            TargetFpsYear = fpsYear
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
        // Computed once, inside the finally block below, only for a genuine (non-cancellation)
        // failure — reused by ThrowWithStructuredLog afterward so the same exception is never
        // classified twice at two different points in this method.
        BatchFailureClassification? failureClassification = null;

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

            // startedAt (captured at the very top of RunAsync, strictly before the
            // TryAcquireLockAsync call and everything leading up to it) is guaranteed no later
            // than the timestamp TryAcquireLockAsync itself used to compute the real expires_at —
            // passed through so the heartbeat's initial local lease-boundary estimate can never
            // be later than the actual database deadline. See RunHeartbeatLoopAsync.
            jobException = await RunJobWithRetryAsync(job, jobName, jobQueueId, startedAt, runtimeTimeoutSeconds, cancellationToken);
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
            record.StackTrace = jobException?.StackTrace;

            // job_queue.errormessage is user-facing and must never carry raw exception text — see
            // the Recreate Summaries grid-history design. DiagnosticSummary (bounded, best-effort)
            // is the only place the exception's own text goes; it flows into the Failed-transition
            // job_queue_log row via JobExecutionRepository, never job_queue itself.
            switch (jobException)
            {
                case null:
                    record.ErrorMessage = null;
                    break;

                case OperationCanceledException:
                    // BatchFailureClassifier.Classify explicitly must not receive a cancellation.
                    record.ErrorMessage = "Job execution was cancelled.";
                    record.DiagnosticSummary = DiagnosticSummaryBuilder.Build(jobException);
                    break;

                default:
                    failureClassification = _failureClassifier.Classify(jobException);
                    record.ErrorMessage = BatchFailureMessageProvider.GetHumanReadableMessage(failureClassification.Category);
                    record.DiagnosticSummary = DiagnosticSummaryBuilder.Build(jobException);
                    break;
            }

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
            var completionContext = new BatchJobCompletionContext(
                jobQueueId, jobExecutionId, jobName, fpsYear, userId, status, ErrorMessage: null);
            await TryNotifyCompletionAsync(completionContext, cancellationToken);
            await TryNotifyExecutionOutcomeAsync(record);
        }

        if (jobException is OperationCanceledException cancelEx)
            throw cancelEx;

        if (jobException != null)
        {
            // Runs after the lock has already been released above — best-effort operational
            // reporting is not part of the protected batch execution and must not delay it.
            // Post-completion notifiers (e.g. the Bulk Rates approver email) fire here too, since
            // their recipients need failure visibility just as much as success — a cancelled run
            // (handled above) is deliberately excluded, not a genuine failure worth alerting on.
            var completionContext = new BatchJobCompletionContext(
                jobQueueId, jobExecutionId, jobName, fpsYear, userId, status, jobException.Message);
            await TryNotifyCompletionAsync(completionContext, cancellationToken);
            await TryNotifyExecutionOutcomeAsync(record);

            // failureClassification is always set here: this branch only runs for jobException !=
            // null, and the OperationCanceledException case already returned above — so every path
            // reaching this line went through the finally block's "default" classification arm.
            ThrowWithStructuredLog(jobException, failureClassification!, jobName, jobQueueId, jobExecutionId);
        }

        return new JobExecutionResult(jobQueueId, jobName, status, finalDuration, executionId);
    }

    private static bool IsWorkerManagedScheduledRun(string jobName, RunMode runMode) =>
        runMode == RunMode.Scheduled &&
        (string.Equals(jobName, BatchJobNames.MabArchive, StringComparison.OrdinalIgnoreCase) ||
         MonthlyScheduledNotificationJobs.Contains(jobName));

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
        IBatchJob job, string jobName, Guid jobQueueId, DateTime startedAtUtc,
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

            // Assigned inside the try block below; default here only so the finally block has a
            // definitely-assigned task to await if cancellationToken.ThrowIfCancellationRequested
            // fires before either task is actually started.
            Task jobTask = Task.CompletedTask;
            Task heartbeatTask = Task.CompletedTask;
            // Set only by the heartbeat-fault branch below, which already reads heartbeatTask's
            // exception directly. Without this flag, the finally block's own await of an
            // already-faulted heartbeatTask would re-throw the same exception and log it a
            // second time as a generic "shutdown" warning, obscuring the specific message already
            // logged above.
            var heartbeatFaultAlreadyReported = false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation(
                    "Executing job '{JobName}' | Attempt={Attempt}/{TotalAttempts}",
                    jobName, attempt, totalAttempts);

                // The heartbeat listens on the same attemptToken the job uses — no separate
                // CancellationTokenSource needed. Runtime timeout / host shutdown already cancel
                // attemptToken today, which stops both; this finally block's existing
                // attemptCts.Cancel() (below) now also doubles as the heartbeat's stop signal once
                // the job finishes first. See the Phase 4 concurrency design note.
                //
                // Heartbeat started before the job, deliberately: an async method runs
                // synchronously up to its first genuinely-incomplete await, so if job.ExecuteAsync
                // were called first and a badly-behaved job blocked in synchronous code (or an
                // await-free loop) before yielding, the line starting the heartbeat would never
                // even run. Starting the heartbeat first doesn't make a cancellation-ignoring job
                // safe, but it does guarantee the lease-maintenance mechanism has begun (its own
                // first await, Task.Delay, yields immediately) before handing control to job code.
                heartbeatTask = RunHeartbeatLoopAsync(jobName, jobQueueId, startedAtUtc, attemptToken);
                jobTask = job.ExecuteAsync(attemptToken);

                // Capturing which task WhenAny actually woke up for, not just checking
                // heartbeatTask.IsFaulted afterward, is required for correctness: Task state is
                // immutable once set, but WhenAny only guarantees at least one task had completed
                // at the moment it resolved — the *other* task's state can still change between
                // that resolution and this line running (scheduling isn't instantaneous). Without
                // capturing the winner, a heartbeat that happens to fault a moment after the job
                // already finished on its own would be misreported as the cause, discarding the
                // job's real (successful or failed) outcome.
                var completedTask = await Task.WhenAny(jobTask, heartbeatTask);

                if (completedTask == heartbeatTask && heartbeatTask.IsFaulted)
                {
                    // The heartbeat can no longer be trusted to keep the lease renewed — the job
                    // must not continue either way, whether this is a definitive lease loss or an
                    // unrelated heartbeat infrastructure failure. Cancel and let the job unwind;
                    // its own outcome (if any) is not what gets reported here — the heartbeat's
                    // failure is the real story.
                    heartbeatFaultAlreadyReported = true;
                    attemptCts.Cancel();
                    try
                    {
                        await jobTask;
                    }
                    catch (Exception jobUnwindEx)
                    {
                        _logger.LogWarning(jobUnwindEx,
                            "Job '{JobName}' threw while unwinding after a heartbeat failure — the heartbeat's own failure is reported instead | Attempt={Attempt}/{TotalAttempts}",
                            jobName, attempt, totalAttempts);
                    }

                    var heartbeatException = heartbeatTask.Exception!.GetBaseException();

                    if (heartbeatException is BatchLockLeaseLostException)
                    {
                        _logger.LogError(heartbeatException,
                            "Job '{JobName}' lock lease was lost mid-execution — cancelled | Attempt={Attempt}/{TotalAttempts} | JobQueueId={JobQueueId}",
                            jobName, attempt, totalAttempts, jobQueueId);
                    }
                    else
                    {
                        _logger.LogError(heartbeatException,
                            "Heartbeat for job '{JobName}' failed unexpectedly — job cancelled as a precaution, since the lease can no longer be trusted to be renewed | Attempt={Attempt}/{TotalAttempts} | JobQueueId={JobQueueId}",
                            jobName, attempt, totalAttempts, jobQueueId);
                    }

                    // Never retried, regardless of the underlying exception's usual retryability —
                    // see IsRetryable and RunHeartbeatLoopAsync's own reasoning.
                    return heartbeatException;
                }

                // Heartbeat did not fault — either the job finished first, or attemptToken was
                // cancelled externally (runtime timeout / host shutdown), which also stops the
                // heartbeat cleanly (its own catch further down observes this as a normal, not
                // faulted, completion). Either way, observe the job's own outcome as before.
                await jobTask;
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
                attemptCts.Cancel();

                // finally always runs, even after the heartbeat-fault branch's `return` above —
                // without this guard, awaiting an already-faulted heartbeatTask here would
                // re-throw and log the *same* exception a second time as a generic "shutdown"
                // warning, obscuring the specific lease-loss/unexpected-failure message already
                // logged there. Skip only in that one case; every other path (job finished first,
                // runtime timeout, host cancellation, a retryable exception) still needs this
                // await to actually wait for heartbeat shutdown and to catch the rare race where
                // the heartbeat faults independently while being stopped.
                if (!heartbeatFaultAlreadyReported)
                {
                    try
                    {
                        await heartbeatTask;
                    }
                    catch (Exception heartbeatShutdownEx)
                    {
                        _logger.LogWarning(heartbeatShutdownEx,
                            "Heartbeat task for job '{JobName}' ended with an exception during shutdown | Attempt={Attempt}/{TotalAttempts}",
                            jobName, attempt, totalAttempts);
                    }
                }
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
    /// <param name="classification">
    /// Already computed by the caller (in <see cref="RunAsync"/>'s <c>finally</c> block) — reused
    /// here rather than re-classifying the same exception a second time.
    /// </param>
    private void ThrowWithStructuredLog(Exception exception, BatchFailureClassification classification, string jobName, Guid jobQueueId, Guid jobExecutionId)
    {
        // Only Sql and General have CloudWatch alarms wired up; Configuration and email
        // failures roll up into General rather than an unwatched alarm channel.
        _logger.LogError(
            exception,
            "[{ErrorType}] Batch job failed | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId}",
            classification.ErrorType,
            jobName,
            jobQueueId,
            jobExecutionId);

        ExceptionDispatchInfo.Capture(exception).Throw();
    }

    /// <summary>
    /// Invokes all registered post-completion notifiers after a job durably reaches Completed or
    /// Failed and its lock is released. Notifier failures are logged and swallowed — a
    /// notification problem must not alter the durable job outcome.
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
    /// Sends a best-effort execution-outcome notification (Completed or Failed) once the final
    /// status is durably persisted and the lock released. Never lets a notification failure
    /// change or mask the persisted outcome. Applies to every job automatically — gated only on
    /// <see cref="BatchAlertingSettings.EnableEmailNotifications"/> and the matching
    /// <see cref="BatchAlertingSettings.NotifyOnSuccess"/>/<see cref="BatchAlertingSettings.NotifyOnFailure"/>
    /// flag, with no per-job allow-list. <see cref="CancellationToken.None"/> is used deliberately —
    /// this runs after the protected execution has already finished, so it must not be tangled up
    /// in that execution's own cancellation.
    /// </summary>
    private async Task TryNotifyExecutionOutcomeAsync(JobExecutionRecord record)
    {
        var shouldNotify = record.Status switch
        {
            JobStatus.Completed => _alertingSettings.NotifyOnSuccess,
            JobStatus.Failed => _alertingSettings.NotifyOnFailure,
            _ => false
        };

        if (!_alertingSettings.EnableEmailNotifications || !shouldNotify)
        {
            return;
        }

        try
        {
            var notification = new BatchExecutionNotification(
                record.JobName,
                record.JobExecutionId,
                record.JobQueueId,
                record.RunMode,
                record.UserId,
                record.RequestedAtUtc,
                record.Status,
                record.CompletedAt ?? DateTime.UtcNow,
                record.DurationSeconds.HasValue ? TimeSpan.FromSeconds(record.DurationSeconds.Value) : null,
                // This is an operational alert, not the public grid — it wants the diagnostic
                // detail (falls back to the friendly ErrorMessage only if none was built), not the
                // now-friendly ErrorMessage a business user sees.
                record.Status == JobStatus.Failed ? record.DiagnosticSummary ?? record.ErrorMessage : null);

            await _notificationService.SendExecutionNotificationAsync(notification, CancellationToken.None);
        }
        catch (Exception notifyEx)
        {
            _logger.LogWarning(
                notifyEx,
                "Failed to send execution notification | JobName={JobName} | JobExecutionId={JobExecutionId} | Status={Status}",
                record.JobName,
                record.JobExecutionId,
                record.Status);
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

        // A lost lease means the attempt loop no longer holds the lock at all — the loop never
        // re-acquires between attempts, so retrying would re-execute business logic without
        // exclusive access. Explicit rather than relying on the `_ => false` default, to document
        // the reasoning (also covers JobLockException generally, since BatchLockLeaseLostException
        // is a subclass, but the explicit case documents intent specifically for lease loss).
        BatchLockLeaseLostException => false,

        ArgumentException => false,
        InvalidOperationException => false,
        NotSupportedException => false,
        NotImplementedException => false,

        // undefined_table: a schema/SQL mismatch, not transient infrastructure — retrying re-runs
        // the same broken query and can never succeed.
        PostgresException pg when pg.SqlState == PostgresErrorCodes.UndefinedTable => false,

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

    /// <summary>
    /// Renews this execution's lock lease on a fixed interval for as long as
    /// <paramref name="cancellationToken"/> stays uncancelled, throwing
    /// <see cref="BatchLockLeaseLostException"/> the moment ownership is definitively lost. Uses
    /// its own independent <see cref="IHeartbeatRepositoryScope"/> — an EF Core DbContext is not
    /// thread-safe, and this loop runs concurrently with the job's own DB work on the ambient
    /// scoped context (see the Phase 4 concurrency design note, §3).
    /// <para>
    /// No catch-all wraps the whole loop: only the individually-scoped renew/touch operations are
    /// caught, per their own semantics below. Anything else (e.g. the DbContext factory failing
    /// at startup) is deliberately left to propagate and fault this method's task — the caller
    /// (<see cref="RunJobWithRetryAsync"/>) treats any fault here as "the lease can no longer be
    /// trusted," not only a genuine <see cref="BatchLockLeaseLostException"/>.
    /// </para>
    /// </summary>
    /// <param name="lockAcquisitionUpperBoundUtc">
    /// A timestamp guaranteed no later than the one <c>TryAcquireLockAsync</c> itself used to
    /// compute the real <c>expires_at</c> — <c>RunAsync</c>'s own <c>startedAt</c>, captured
    /// before that call and everything leading up to it. Used only to seed the locally-tracked
    /// lease boundary conservatively; never read back from the database.
    /// </param>
    private async Task RunHeartbeatLoopAsync(string jobName, Guid jobQueueId, DateTime lockAcquisitionUpperBoundUtc, CancellationToken cancellationToken)
    {
        var lockName = ResolveLockName(jobName);

        await using var heartbeatScope = _heartbeatRepositoryScopeFactory.Create();
        var lockRepository = heartbeatScope.LockRepository;
        var executionRepository = heartbeatScope.ExecutionRepository;

        // Tracks this loop's own best estimate of when the current lease expires, purely to
        // decide when a *thrown* renewal (as opposed to a clean `false`) has gone on long enough
        // to treat as loss rather than a transient blip. Not re-read from the database — deliberately
        // conservative instead: seeded from a timestamp guaranteed no later than the real
        // acquisition time, so this local deadline can only be earlier than (or equal to) the
        // actual database expires_at, never later. A too-early local deadline just means an
        // occasional unnecessary lease-loss report; a too-late one would mean this worker could
        // believe it still owns a lease that another worker has already legitimately reclaimed.
        var leaseExpiresAtUtc = lockAcquisitionUpperBoundUtc.AddSeconds(_lockTimeoutSeconds);

        while (true)
        {
            // Cancellation here (runtime timeout, host shutdown, or the job finishing first and
            // the caller cancelling attemptCts) propagates naturally — no explicit catch needed;
            // .NET marks the resulting task Canceled, not Faulted, since the thrown token matches
            // the one this method itself observes.
            await Task.Delay(TimeSpan.FromSeconds(_heartbeatIntervalSeconds), cancellationToken);

            // Captured before the call, not after: TryRenewLockAsync's own implementation
            // computes the real expires_at from a `now` read at its own start, before its DB
            // round-trip. Capturing our local estimate afterward (post-await) would make it
            // systematically later than the real value by roughly that round-trip's latency —
            // capturing it before guarantees the opposite, safe direction (see the field comment
            // above and fix #2 from the Phase 4 review).
            var renewalStartedAtUtc = DateTime.UtcNow;
            bool renewed;
            try
            {
                renewed = await lockRepository.TryRenewLockAsync(lockName, jobQueueId, _lockTimeoutSeconds, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A thrown renewal is NOT automatic ownership loss — the lease itself hasn't
                // necessarily expired, only this attempt to renew it failed to execute (e.g. a
                // transient DB/network blip). Retry on the next tick as long as the last known
                // lease is still within its window; only once that boundary passes without a
                // successful renewal does this become a loss.
                _logger.LogWarning(ex,
                    "Heartbeat renewal threw for job '{JobName}' — retrying while the last known lease is still valid | JobQueueId={JobQueueId} | LeaseExpiresAtUtc={LeaseExpiresAtUtc}",
                    jobName, jobQueueId, leaseExpiresAtUtc);

                if (DateTime.UtcNow >= leaseExpiresAtUtc)
                {
                    throw new BatchLockLeaseLostException(
                        $"Lock renewal for job '{jobName}' (lock '{lockName}') kept failing until the lease boundary ({leaseExpiresAtUtc:O}) passed without a successful renewal.",
                        ex);
                }

                continue;
            }

            if (!renewed)
            {
                // Definitive: Phase 1's `expires_at > now()` renewal guard means an expired lease
                // can never be renewed by anyone, including its own owner, so a clean `false` can
                // only mean ownership is already gone.
                throw new BatchLockLeaseLostException(
                    $"Lock renewal for job '{jobName}' (lock '{lockName}') returned false — ownership has already been lost.");
            }

            leaseExpiresAtUtc = renewalStartedAtUtc.AddSeconds(_lockTimeoutSeconds);

            try
            {
                // Secondary — the lock renewal above is what proves ownership; updated_at is
                // observability, not correctness. A failure here (thrown, or a clean `false`)
                // never escalates.
                var touched = await executionRepository.TouchRunningExecutionAsync(jobQueueId, cancellationToken);
                if (!touched)
                {
                    _logger.LogWarning(
                        "Heartbeat could not update job_queue.updated_at for job '{JobName}' (row no longer Running) — lock ownership unaffected | JobQueueId={JobQueueId}",
                        jobName, jobQueueId);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Heartbeat's TouchRunningExecutionAsync failed for job '{JobName}' — lock renewal already succeeded, ownership unaffected | JobQueueId={JobQueueId}",
                    jobName, jobQueueId);
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
