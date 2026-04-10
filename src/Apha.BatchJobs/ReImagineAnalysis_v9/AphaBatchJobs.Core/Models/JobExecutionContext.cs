namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the execution context for a batch job, containing metadata about the job run.
/// </summary>
/// <param name="JobName">The name of the job being executed.</param>
/// <param name="CorrelationId">The unique correlation identifier for tracking the execution across logs and systems.</param>
/// <param name="TriggerType">The type of trigger that initiated the job execution (Scheduled or Adhoc).</param>
/// <param name="StartedAt">The timestamp when the job execution started.</param>
public sealed record JobExecutionContext(
    string JobName,
    string CorrelationId,
    Enums.JobType TriggerType,
    DateTimeOffset StartedAt
)
{
    // Validation to ensure required properties are not null or empty
    public JobExecutionContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(JobName, nameof(JobName));
        ArgumentException.ThrowIfNullOrWhiteSpace(CorrelationId, nameof(CorrelationId));
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to prevent inheritance, which is a best practice for records unless inheritance is explicitly needed
// 2. Added validation in the primary constructor to ensure JobName and CorrelationId are not null or whitespace
// 3. Used ArgumentException.ThrowIfNullOrWhiteSpace (available in .NET 8) for concise null/empty validation
// 4. Maintained the existing functionality without adding new features
// 5. The record pattern with positional parameters is already idiomatic for .NET 8
// 6. DateTimeOffset is appropriate for distributed systems (AWS) as it includes timezone information