using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;

/// <summary>
/// Rebuilds FPS totals for the previous year.
/// </summary>
public sealed class ProcessPreviousYearTotalsHandler : IScheduledLoadFromFpsStepHandler
{
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ILogger<ProcessPreviousYearTotalsHandler> _logger;

    public ProcessPreviousYearTotalsHandler(
        IScheduledLoadFromFpsRepository repository,
        ILogger<ProcessPreviousYearTotalsHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ScheduledLoadFromFpsStep Step => ScheduledLoadFromFpsStep.ProcessPreviousYearTotals;

    public async Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken)
    {
        var rowsAffected = await _repository.RebuildYearTotalsAsync(context.PreviousYear, cancellationToken);
        _logger.LogInformation(
            "Rebuilt previous year totals | Year={Year} | RowsAffected={RowsAffected}",
            context.PreviousYear,
            rowsAffected);

        return rowsAffected;
    }
}
