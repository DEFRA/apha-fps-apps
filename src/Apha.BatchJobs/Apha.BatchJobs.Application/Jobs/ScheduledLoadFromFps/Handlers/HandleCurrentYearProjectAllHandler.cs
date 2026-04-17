using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;

/// <summary>
/// Refreshes current year project snapshot in mabarchive targets.
/// </summary>
public sealed class HandleCurrentYearProjectAllHandler : IScheduledLoadFromFpsStepHandler
{
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ILogger<HandleCurrentYearProjectAllHandler> _logger;

    public HandleCurrentYearProjectAllHandler(
        IScheduledLoadFromFpsRepository repository,
        ILogger<HandleCurrentYearProjectAllHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ScheduledLoadFromFpsStep Step => ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll;

    public async Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken)
    {
        var rowsAffected = await _repository.RefreshCurrentYearProjectAllAsync(context.CurrentYear, cancellationToken);
        _logger.LogInformation(
            "Refreshed current year project snapshot | Year={Year} | RowsAffected={RowsAffected}",
            context.CurrentYear,
            rowsAffected);

        return rowsAffected;
    }
}
