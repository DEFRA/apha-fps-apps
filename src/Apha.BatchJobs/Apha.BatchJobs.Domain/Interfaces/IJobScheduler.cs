namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Contract for scheduled job host lifecycle management.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Starts scheduler infrastructure.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops scheduler infrastructure.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);
}
