using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;

/// <summary>
/// Performs legacy-style year-slice delete across archive targets before reloading data.
/// </summary>
public sealed class DeleteYearsFpsDataHandler : IScheduledLoadFromFpsStepHandler
{
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ILogger<DeleteYearsFpsDataHandler> _logger;

    public DeleteYearsFpsDataHandler(
        IScheduledLoadFromFpsRepository repository,
        ILogger<DeleteYearsFpsDataHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ScheduledLoadFromFpsStep Step => ScheduledLoadFromFpsStep.DeleteYearsFpsData;

    public async Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken)
    {
        var years = GetTargetYears(context);
        var totalRowsAffected = 0;

        foreach (var year in years)
        {
            totalRowsAffected += await _repository.DeleteArchiveYearSliceAsync(year, cancellationToken);
        }

        _logger.LogInformation(
            "Deleted archive year slices | Years={Years} | RowsAffected={RowsAffected}",
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
