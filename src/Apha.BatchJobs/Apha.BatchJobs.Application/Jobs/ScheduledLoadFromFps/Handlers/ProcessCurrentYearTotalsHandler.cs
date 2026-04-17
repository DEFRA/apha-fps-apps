using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;

/// <summary>
/// Rebuilds FPS totals for the current year when cutover has passed.
/// </summary>
public sealed class ProcessCurrentYearTotalsHandler : IScheduledLoadFromFpsStepHandler
{
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ILogger<ProcessCurrentYearTotalsHandler> _logger;

    public ProcessCurrentYearTotalsHandler(
        IScheduledLoadFromFpsRepository repository,
        ILogger<ProcessCurrentYearTotalsHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ScheduledLoadFromFpsStep Step => ScheduledLoadFromFpsStep.ProcessCurrentYearTotals;

    public async Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken)
    {
        var rowsAffected = await _repository.RebuildYearTotalsAsync(context.CurrentYear, cancellationToken);
        _logger.LogInformation(
            "Rebuilt current year totals | Year={Year} | RowsAffected={RowsAffected}",
            context.CurrentYear,
            rowsAffected);

        return rowsAffected;
    }
}
