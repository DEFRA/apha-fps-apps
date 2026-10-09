namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>
/// Fails job_queue rows that were dispatched to the worker but never claimed within their time
/// to live, so a request that no container picked up stops blocking new requests of that job.
/// </summary>
public interface IDispatchTimeoutSweepService
{
    /// <summary>
    /// Runs one sweep over every job that waits for the worker in a dispatched status. Throws only
    /// on cancellation: a failure for one job is logged and the remaining jobs are still swept.
    /// </summary>
    Task SweepAsync(CancellationToken cancellationToken = default);
}
