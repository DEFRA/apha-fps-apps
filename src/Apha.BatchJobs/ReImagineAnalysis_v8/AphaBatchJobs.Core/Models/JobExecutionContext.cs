namespace AphaBatchJobs.Core.Models;

using AphaBatchJobs.Core.Enums;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the execution context for a batch job.
/// This record encapsulates all contextual information needed to track and execute a job,
/// including identification, correlation tracking, trigger type, and timing information.
/// </summary>
/// <param name="JobName">The name of the job being executed. Used for identification and logging purposes.</param>
/// <param name="CorrelationId">A unique identifier for tracking this specific job execution across logs and systems.</param>
/// <param name="TriggerType">Indicates whether this job was triggered as a Scheduled or Adhoc execution.</param>
/// <param name="StartedAt">The timestamp when the job execution began, using DateTimeOffset for timezone awareness.</param>
public sealed record JobExecutionContext(
    [Required, MaxLength(255)] string JobName,
    [Required, MaxLength(100)] string CorrelationId,
    JobType TriggerType,
    DateTimeOffset StartedAt
)
{
    // Best Practice: Add validation to ensure data integrity
    // This constructor validates the input parameters to prevent invalid state
    public JobExecutionContext(
        string jobName,
        string correlationId,
        JobType triggerType,
        DateTimeOffset startedAt) : this(jobName, correlationId, triggerType, startedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName, nameof(jobName));
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId, nameof(correlationId));
        
        // Validate that StartedAt is not in the future (with small tolerance for clock skew)
        if (startedAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            throw new ArgumentException("StartedAt cannot be in the future.", nameof(startedAt));
        }
    }
    
    // Best Practice: Provide a factory method for creating new contexts with UTC timestamp
    public static JobExecutionContext Create(string jobName, string correlationId, JobType triggerType)
    {
        return new JobExecutionContext(jobName, correlationId, triggerType, DateTimeOffset.UtcNow);
    }
}