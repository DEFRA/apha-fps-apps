using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Default implementation that creates the canonical 5-step orchestration plan.
/// </summary>
public sealed class ScheduledLoadFromFpsPlanBuilder : IScheduledLoadFromFpsPlanBuilder
{
    private const int DefaultCutoverMonth = 4;
    private readonly ScheduledLoadFromFpsSettings _settings;

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduledLoadFromFpsPlanBuilder"/>.
    /// </summary>
    /// <param name="settings">Scheduled job settings.</param>
    public ScheduledLoadFromFpsPlanBuilder(IOptions<ScheduledLoadFromFpsSettings> settings)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public ScheduledLoadFromFpsExecutionPlan Build()
    {
        var utcNow = DateTime.UtcNow;
        var cutoverMonth = NormalizeMonth(_settings.CurrentYearCutoverMonth, DefaultCutoverMonth);
        var currentMonth = NormalizeMonth(_settings.ForceCurrentMonth, utcNow.Month);
        var currentYear = _settings.ForceCurrentYear ?? utcNow.Year;
        var context = new ScheduledLoadFromFpsExecutionContext(
            currentMonth,
            currentYear,
            currentYear - 1,
            cutoverMonth);

        var steps = new List<ScheduledLoadFromFpsStep>
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals
        };

        if (context.CurrentMonth > context.CurrentYearCutoverMonth)
        {
            steps.Add(ScheduledLoadFromFpsStep.ProcessCurrentYearTotals);
        }

        steps.Add(ScheduledLoadFromFpsStep.DeleteYearsFpsData);
        steps.Add(ScheduledLoadFromFpsStep.AddYearsFpsData);
        steps.Add(ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll);

        return new ScheduledLoadFromFpsExecutionPlan(context, steps);
    }

    private static int NormalizeMonth(int? candidate, int fallback)
    {
        if (!candidate.HasValue)
        {
            return fallback;
        }

        return candidate.Value is >= 1 and <= 12 ? candidate.Value : fallback;
    }
}
