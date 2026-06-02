namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Starts batch jobs asynchronously for API trigger requests.
/// </summary>
public interface IJobTriggerService
{
    /// <summary>
    /// Queues a job run and returns a job execution identifier immediately.
    /// </summary>
    Task<TriggerResult> TriggerAsync(string jobName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result returned after accepting a trigger request.
/// </summary>
public sealed record TriggerResult(string JobExecutionId, DateTime AcceptedAtUtc);
