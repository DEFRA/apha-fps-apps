using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Infrastructure.Context;

/// <summary>
/// Scoped holder for RecreateSummaries run parameters.
/// </summary>
public sealed class RecreateSummariesContext : IRecreateSummariesContext
{
    /// <summary>
    /// Initializes context from optional environment overrides.
    /// Defaults remain month=1 and triggeredBy=system for backward compatibility.
    /// </summary>
    public RecreateSummariesContext()
    {
        var monthOverride = Environment.GetEnvironmentVariable("BATCH_RECREATE_SUMMARIES_MONTH");
        if (int.TryParse(monthOverride, out var parsedMonth) && parsedMonth is >= 1 and <= 12)
        {
            Month = parsedMonth;
        }

        var triggeredByOverride = Environment.GetEnvironmentVariable("BATCH_RECREATE_SUMMARIES_TRIGGERED_BY");
        if (!string.IsNullOrWhiteSpace(triggeredByOverride))
        {
            TriggeredBy = triggeredByOverride.Trim();
        }
    }

    /// <inheritdoc />
    public int Month { get; set; } = 1;

    /// <inheritdoc />
    public string TriggeredBy { get; set; } = "system";
}
