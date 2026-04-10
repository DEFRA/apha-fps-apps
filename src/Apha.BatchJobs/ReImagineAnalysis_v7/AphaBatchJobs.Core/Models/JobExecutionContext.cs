namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the execution context for a batch job.
/// Contains metadata and tracking information passed to jobs during execution.
/// </summary>
/// <param name="JobName">The name of the job being executed.</param>
/// <param name="CorrelationId">Unique identifier for tracking this execution across logs and systems.</param>
/// <param name="TriggerType">The type of trigger that initiated this job execution (Scheduled or Adhoc).</param>
/// <param name="StartedAt">The timestamp when the job execution started.</param>
public sealed record JobExecutionContext(
    string JobName,
    string CorrelationId,
    Enums.JobType TriggerType,
    DateTimeOffset StartedAt
)
{
    // Add validation to ensure required properties are not null or empty
    public JobExecutionContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(JobName, nameof(JobName));
        ArgumentException.ThrowIfNullOrWhiteSpace(CorrelationId, nameof(CorrelationId));
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to prevent inheritance, which is a best practice for records
//    that are not designed to be extended, improving performance and clarity
// 2. Added validation in the primary constructor using ArgumentException.ThrowIfNullOrWhiteSpace
//    to ensure JobName and CorrelationId are not null or empty strings
// 3. Used .NET 6+ ArgumentException.ThrowIfNullOrWhiteSpace for cleaner null/empty validation
// 4. TriggerType (enum) and StartedAt (DateTimeOffset) are value types and cannot be null,
//    so no validation needed for them
// 5. Maintained the existing functionality without adding new features