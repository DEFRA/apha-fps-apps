namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Dispatches batch job trigger requests to the configured cloud transport.
/// </summary>
public interface IJobDispatchService
{
    /// <summary>
    /// Dispatches the given job name and returns a transport-specific dispatch id.
    /// </summary>
    Task<string> RunBatchJobAsync(string jobName, CancellationToken cancellationToken = default);
}
