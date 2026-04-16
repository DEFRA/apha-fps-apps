using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Worker.Services;

/// <summary>
/// Generic-host wrapper for scheduler lifecycle integration.
/// </summary>
public sealed class SchedulerHostedService : IHostedService
{
    private readonly IJobScheduler _jobScheduler;
    private readonly ILogger<SchedulerHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerHostedService"/> class.
    /// </summary>
    public SchedulerHostedService(IJobScheduler jobScheduler, ILogger<SchedulerHostedService> logger)
    {
        _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scheduler hosted service");
        await _jobScheduler.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping scheduler hosted service");
        await _jobScheduler.StopAsync(cancellationToken);
    }
}
