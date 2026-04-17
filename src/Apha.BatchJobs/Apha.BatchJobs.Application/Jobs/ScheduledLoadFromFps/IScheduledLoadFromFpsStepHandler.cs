namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Executes a single structured step in the ScheduledLoadFromFps flow.
/// </summary>
public interface IScheduledLoadFromFpsStepHandler
{
    ScheduledLoadFromFpsStep Step { get; }

    Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken);
}
