namespace AphaBatchJobs.Core.Models;

using AphaBatchJobs.Core.Enums;

/// <summary>
/// Represents the execution context for a batch job.
/// Contains all necessary information to track and correlate a job execution instance.
/// </summary>
/// <param name="JobName">The unique name identifying the job being executed.</param>
/// <param name="CorrelationId">A unique identifier for correlating logs and tracking this specific execution.</param>
/// <param name="TriggerType">The type of trigger that initiated this job execution (Scheduled or Adhoc).</param>
/// <param name="StartedAt">The timestamp when the job execution started.</param>
public sealed record JobExecutionContext(
    string JobName,
    string CorrelationId,
    JobType TriggerType,
    DateTimeOffset StartedAt
);
