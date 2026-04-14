using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application;

/// <summary>
/// Implements the full execution lifecycle for a batch job:
/// generate RunId → acquire lock → record start → execute → record result → release lock.
/// </summary>
public sealed class JobOrchestrator : IJobOrchestrator
{
    private readonly IBatchJobFactory _factory;
    private readonly IBatchLockRepository _lockRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<JobOrchestrator> _logger;

    /// <summary>Lock timeout in seconds — matches ECS task timeout so stale locks always expire.</summary>
    private const int LockTimeoutSeconds = 3600;

    /// <summary>
    /// Initializes a new instance of <see cref="JobOrchestrator"/>.
    /// </summary>
    public JobOrchestrator(
        IBatchJobFactory factory,
        IBatchLockRepository lockRepository,
        IJobExecutionRepository executionRepository,
        ILogger<JobOrchestrator> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _lockRepository = lockRepository ?? throw new ArgumentNullException(nameof(lockRepository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<JobExecutionResult> RunAsync(
        string jobName,
        RunMode runMode,
        CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid().ToString("N");
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation("--- Orchestrator: Starting '{JobName}' | RunId={RunId} | Mode={RunMode}",
            jobName, runId, runMode);

        // Step 1 — Acquire distributed lock
        _logger.LogInformation("Acquiring execution lock for '{JobName}'...", jobName);
        var lockAcquired = await _lockRepository.TryAcquireLockAsync(
            jobName, runId, LockTimeoutSeconds, cancellationToken);

        if (!lockAcquired)
        {
            _logger.LogWarning(
                "Job '{JobName}' is already running (lock held by another process). Skipping this run.",
                jobName);
            return new JobExecutionResult(runId, jobName, JobStatus.Skipped, TimeSpan.Zero);
        }

        _logger.LogInformation("Lock acquired for '{JobName}' | RunId={RunId}", jobName, runId);

        // Step 2 — Create execution record (Started)
        var record = new JobExecutionRecord
        {
            ExecutionId = 0,   // DB assigns real ID on insert
            JobName = jobName,
            RunId = runId,
            JobType = JobType.Unknown,
            RunMode = runMode,
            Status = JobStatus.Running,
            StartedAt = startedAt
        };

        int executionId = 0;
        try
        {
            executionId = await _executionRepository.CreateExecutionRecordAsync(record, cancellationToken);
            record.ExecutionId = executionId;
            _logger.LogInformation("Execution record created | ExecutionId={ExecutionId}", executionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not write execution start record — continuing without tracking");
        }

        // Step 3 — Execute the job
        var job = _factory.Create(jobName);
        Exception? jobException = null;

        try
        {
            _logger.LogInformation("Executing job '{JobName}'...", jobName);
            await job.ExecuteAsync(cancellationToken);
            _logger.LogInformation("Job '{JobName}' completed successfully", jobName);
        }
        catch (OperationCanceledException ex)
        {
            jobException = ex;
            _logger.LogWarning(ex, "Job '{JobName}' was cancelled", jobName);
        }
        catch (Exception ex)
        {
            jobException = ex;
            _logger.LogError(ex, "Job '{JobName}' failed: {ErrorMessage}", jobName, ex.Message);
        }
        finally
        {
            // Step 4 — Update execution record (Completed or Failed)
            var completedAt = DateTime.UtcNow;
            var duration = completedAt - startedAt;
            var finalStatus = jobException switch
            {
                null => JobStatus.Completed,
                OperationCanceledException => JobStatus.Cancelled,
                _ => JobStatus.Failed
            };

            record.Status = finalStatus;
            record.CompletedAt = completedAt;
            record.DurationSeconds = (int)duration.TotalSeconds;
            record.ErrorMessage = jobException?.Message;
            record.StackTrace = jobException?.StackTrace;

            try
            {
                await _executionRepository.UpdateExecutionRecordAsync(record, cancellationToken);
                _logger.LogInformation(
                    "Execution record updated | Status={Status} | Duration={DurationSeconds}s",
                    finalStatus, record.DurationSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not write execution completion record — job result may not be persisted");
            }

            // Step 5 — Release lock (always)
            try
            {
                await _lockRepository.ReleaseLockAsync(jobName, runId, cancellationToken);
                _logger.LogInformation("Lock released for '{JobName}' | RunId={RunId}", jobName, runId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not release lock for '{JobName}' | RunId={RunId} — lock will expire after {Timeout}s",
                    jobName, runId, LockTimeoutSeconds);
            }
        }

        var finalDuration = DateTime.UtcNow - startedAt;
        var status = jobException switch
        {
            null => JobStatus.Completed,
            OperationCanceledException => JobStatus.Cancelled,
            _ => JobStatus.Failed
        };

        _logger.LogInformation(
            "--- Orchestrator: '{JobName}' finished | Status={Status} | Duration={Duration:mm\\:ss\\.fff} | RunId={RunId}",
            jobName, status, finalDuration, runId);

        if (jobException is OperationCanceledException cancelEx)
            throw cancelEx;

        if (jobException != null)
            throw jobException;

        return new JobExecutionResult(runId, jobName, status, finalDuration, executionId);
    }
}
