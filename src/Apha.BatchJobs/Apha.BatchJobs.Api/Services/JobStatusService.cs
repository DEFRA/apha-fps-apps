using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Implements job status queries by reading from the lock and execution repositories.
/// </summary>
public sealed class JobStatusService : IJobStatusService
{
    private readonly IBatchLockRepository _lockRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IBatchJobFactory _jobFactory;

    public JobStatusService(
        IBatchLockRepository lockRepository,
        IJobExecutionRepository executionRepository,
        IBatchJobFactory jobFactory)
    {
        _lockRepository = lockRepository;
        _executionRepository = executionRepository;
        _jobFactory = jobFactory;
    }

    /// <inheritdoc />
    public async Task<JobStatusResult> GetStatusAsync(string jobName, CancellationToken cancellationToken = default)
    {
        var lockTask = _lockRepository.GetActiveLockAsync(jobName, cancellationToken);
        var execTask = _executionRepository.GetLastExecutionAsync(jobName, cancellationToken);
        await Task.WhenAll(lockTask, execTask);

        var activeLock = await lockTask;
        var lastExec = await execTask;

        return new JobStatusResult
        {
            JobName = jobName,
            IsRunning = activeLock is { IsActive: true },
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
                    Status = lastExec.Status.ToString(),
                    StartedAt = lastExec.StartedAt,
                    CompletedAt = lastExec.CompletedAt
                }
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JobStatusResult>> GetAllStatusesAsync(CancellationToken cancellationToken = default)
    {
        var jobNames = _jobFactory.GetAvailableJobs();

        var tasks = jobNames.Select(name => GetStatusAsync(name, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results;
    }
}
