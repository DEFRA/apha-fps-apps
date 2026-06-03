using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Implements job status queries by reading from the lock and execution repositories.
/// </summary>
public sealed class JobStatusService : IJobStatusService
{
    private readonly IBatchLockRepository _lockRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IBatchJobFactory _jobFactory;
    private readonly StartupWatchdogOptions _watchdogOptions;
    private readonly IHostEnvironment _hostEnvironment;

    public JobStatusService(
        IBatchLockRepository lockRepository,
        IJobExecutionRepository executionRepository,
        IBatchJobFactory jobFactory,
        IOptions<StartupWatchdogOptions> watchdogOptions,
        IHostEnvironment hostEnvironment)
    {
        _lockRepository = lockRepository;
        _executionRepository = executionRepository;
        _jobFactory = jobFactory;
        _watchdogOptions = watchdogOptions.Value;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public async Task<JobStatusResult> GetStatusAsync(
        string jobName,
        Guid? jobExecutionId = null,
        DateTime? acceptedAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var lockTask = _lockRepository.GetActiveLockAsync(jobName, cancellationToken);
        var execTask = jobExecutionId.HasValue
            ? _executionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId.Value, cancellationToken)
            : _executionRepository.GetLastExecutionAsync(jobName, cancellationToken);
        await Task.WhenAll(lockTask, execTask);

        var activeLock = await lockTask;
        var lastExec = await execTask;

        if (jobExecutionId.HasValue && lastExec is not null && !string.Equals(lastExec.JobName, jobName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Execution '{jobExecutionId.Value:D}' does not belong to job '{jobName}'.");
        }

        var watchdog = BuildWatchdogProjection(lastExec, jobExecutionId, acceptedAtUtc);

        return new JobStatusResult
        {
            JobName = jobName,
            IsRunning = activeLock is { IsActive: true },
            StartupWatchdog = watchdog,
            CorrelatedJobExecutionId = lastExec?.JobExecutionId ?? jobExecutionId,
            SourceOfTruth = "BatchJobs",
            ActiveLock = activeLock is { IsActive: true }
                ? new ActiveLockInfo
                {
                    JobQueueId = activeLock.JobQueueId,
                    AcquiredAt = activeLock.AcquiredAt,
                    ExpiresAt = activeLock.ExpiresAt
                }
                : null,
            LastExecution = lastExec is null
                ? null
                : new LastExecutionInfo
                {
                    JobQueueId = lastExec.JobQueueId,
                    JobExecutionId = lastExec.JobExecutionId,
                    Status = lastExec.Status.ToString(),
                    StartedAt = lastExec.StartedAt,
                    CompletedAt = lastExec.CompletedAt
                }
        };
    }

    /// <inheritdoc />
    public async Task<JobStatusResult?> GetStatusByExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        var execution = await _executionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId, cancellationToken);
        if (execution is null)
            return null;

        return await GetStatusAsync(execution.JobName, jobExecutionId, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JobStatusResult>> GetAllStatusesAsync(CancellationToken cancellationToken = default)
    {
        var jobNames = _jobFactory.GetAvailableJobs();

        var tasks = jobNames.Select(name => GetStatusAsync(name, null, null, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results;
    }

    private StartupWatchdogInfo? BuildWatchdogProjection(
        Domain.Entities.JobExecutionRecord? execution,
        Guid? jobExecutionId,
        DateTime? acceptedAtUtc)
    {
        // Startup watchdog projection is relevant only for correlated trigger tracking.
        if (!jobExecutionId.HasValue || !acceptedAtUtc.HasValue || execution is not null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var slaSeconds = ResolveStartupSlaSeconds();
        var deadline = acceptedAtUtc.Value.AddSeconds(slaSeconds);
        var projectedState = now > deadline ? "StartFailedTimeout" : "TriggerAcceptedPendingStart";

        return new StartupWatchdogInfo
        {
            ProjectedState = projectedState,
            AcceptedAtUtc = acceptedAtUtc.Value,
            StartupDeadlineUtc = deadline,
            EvaluatedAtUtc = now,
            StartupSlaSeconds = slaSeconds,
            DeliveryExhaustionConfirmed = false,
            DeliveryExhaustionOwner = "IntegrationTransportReconciler"
        };
    }

    private int ResolveStartupSlaSeconds()
    {
        var configured = _hostEnvironment.IsProduction()
            ? _watchdogOptions.StartupSlaSecondsProduction
            : _watchdogOptions.StartupSlaSecondsNonProduction;

        return Math.Max(1, configured);
    }
}
