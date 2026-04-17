using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;

/// <summary>
/// Performs legacy-style year-slice reload for archive targets.
/// </summary>
public sealed class AddYearsFpsDataHandler : IScheduledLoadFromFpsStepHandler
{
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ILogger<AddYearsFpsDataHandler> _logger;

    public AddYearsFpsDataHandler(
        IScheduledLoadFromFpsRepository repository,
        ILogger<AddYearsFpsDataHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ScheduledLoadFromFpsStep Step => ScheduledLoadFromFpsStep.AddYearsFpsData;

    public async Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken)
    {
        var years = GetTargetYears(context);
        var totalRowsAffected = 0;

        foreach (var year in years)
        {
            totalRowsAffected += await _repository.AddArchiveYearSliceAsync(year, cancellationToken);
        }

        _logger.LogInformation(
            "Loaded archive year slices | Years={Years} | RowsAffected={RowsAffected}",
            string.Join(",", years),
            totalRowsAffected);

        return totalRowsAffected;
    }

    private static IReadOnlyList<int> GetTargetYears(ScheduledLoadFromFpsExecutionContext context)
    {
        var years = new List<int> { context.PreviousYear };
        if (context.CurrentMonth > context.CurrentYearCutoverMonth)
        {
            years.Add(context.CurrentYear);
        }

        return years;
    }
}
