namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Dispatches batch jobs by starting an ECS Fargate task with the job name
/// injected via the BATCH_JOB_NAME container environment variable override.
/// </summary>
public interface IEcsTaskDispatcher
{
    /// <summary>
    /// Starts an ECS Fargate task for the given job name and returns the task ARN.
    /// </summary>
    Task<string> RunBatchJobAsync(string jobName, CancellationToken cancellationToken = default);
}
