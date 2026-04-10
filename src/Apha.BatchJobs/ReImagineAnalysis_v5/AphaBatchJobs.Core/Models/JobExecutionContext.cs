namespace AphaBatchJobs.Core.Models
{
    using AphaBatchJobs.Core.Enums;
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Immutable record representing the execution context for a batch job.
    /// Contains all metadata required to track and correlate job execution across the system.
    /// This context is passed to all job implementations and orchestrators.
    /// </summary>
    /// <param name="JobName">The unique name identifying the job being executed</param>
    /// <param name="CorrelationId">Unique identifier for correlating logs and tracking execution flow</param>
    /// <param name="TriggerType">The type of trigger that initiated this job execution (Scheduled or Adhoc)</param>
    /// <param name="StartedAt">The timestamp when the job execution was initiated</param>
    public sealed record JobExecutionContext(
        [Required, StringLength(200, MinimumLength = 1)] string JobName,
        [Required, StringLength(100, MinimumLength = 1)] string CorrelationId,
        JobType TriggerType,
        DateTimeOffset StartedAt)
    {
        /// <summary>
        /// Validates the JobExecutionContext properties.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        public JobExecutionContext
        {
            get
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(JobName, nameof(JobName));
                ArgumentException.ThrowIfNullOrWhiteSpace(CorrelationId, nameof(CorrelationId));
                
                if (!Enum.IsDefined(typeof(JobType), TriggerType))
                {
                    throw new ArgumentException($"Invalid JobType value: {TriggerType}", nameof(TriggerType));
                }

                if (StartedAt > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    throw new ArgumentException("StartedAt cannot be in the future", nameof(StartedAt));
                }

                return this;
            }
            init { }
        }
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to prevent inheritance (best practice for records that shouldn't be extended)
// 2. Added explicit 'using System;' for better clarity
// 3. Added validation attributes ([Required], [StringLength]) for data validation
// 4. Added property validation in the primary constructor to ensure data integrity
// 5. Validates that JobName and CorrelationId are not null or whitespace
// 6. Validates that TriggerType is a valid enum value
// 7. Validates that StartedAt is not unreasonably in the future (with 5-minute tolerance for clock skew in distributed systems)
// 8. These validations are critical for AWS ECS Fargate environments where jobs may be triggered from various sources
// 9. Proper validation prevents invalid data from propagating through the system and causing issues in PostgreSQL persistence