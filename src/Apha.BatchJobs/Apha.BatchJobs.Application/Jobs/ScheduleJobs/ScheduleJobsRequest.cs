namespace Apha.BatchJobs.Application.Jobs.ScheduleJobs;

/// <summary>
/// Request model for the schedule jobs batch job.
/// Foundation layer placeholder: structure defined for future use.
/// </summary>
public sealed class ScheduleJobsRequest
{
    /// <summary>
    /// Scope of the scheduling operation (e.g., 'All', 'Partial', 'Incremental').
    /// </summary>
    public string Scope { get; set; } = "All";

    /// <summary>
    /// Optional filter parameters for targeted scheduling.
    /// </summary>
    public string? FilterCriteria { get; set; }

    /// <summary>
    /// Whether to perform a dry-run without persisting changes.
    /// </summary>
    public bool DryRun { get; set; } = false;
}
