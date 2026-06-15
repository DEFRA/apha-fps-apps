namespace Apha.BatchJobs.Application.Jobs.RecreateSummaries;

/// <summary>
/// Request model for the recreate summaries batch job.
/// Foundation layer placeholder: structure defined for future use.
/// </summary>
public sealed class RecreateSummariesRequest
{
    /// <summary>
    /// User identifier who triggered the recreation.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Types of summaries to recreate (e.g., 'Daily', 'Weekly', 'Monthly', 'All').
    /// </summary>
    public string SummaryType { get; set; } = "All";

    /// <summary>
    /// Date range start for partitioned recreation.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Date range end for partitioned recreation.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Whether to perform a dry-run without persisting changes.
    /// </summary>
    public bool DryRun { get; set; } = false;
}
