namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Settings for the ScheduledLoadFromFps batch job.
/// </summary>
public sealed class ScheduledLoadFromFpsSettings
{
    /// <summary>
    /// Per-step timeout in seconds.
    /// </summary>
    public int StepTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Month threshold used by the orchestration branching rules.
    /// If current month is greater than this value, current-year totals are processed.
    /// </summary>
    public int CurrentYearCutoverMonth { get; set; } = 4;

    /// <summary>
    /// Optional explicit override for current month during controlled backfills.
    /// Null means use UTC current month.
    /// </summary>
    public int? ForceCurrentMonth { get; set; }

    /// <summary>
    /// Optional explicit override for current year during controlled backfills.
    /// Null means use UTC current year.
    /// </summary>
    public int? ForceCurrentYear { get; set; }

}