using System;

namespace AphaBatchJobsFoundationV3.Core.Models
{
    /// <summary>
    /// Model class representing correlation context for distributed tracing and logging 
    /// throughout batch job execution lifecycle.
    /// This context is used to track and correlate log entries across the entire execution flow.
    /// </summary>
    public class CorrelationContext
    {
        /// <summary>
        /// Gets or sets the unique correlation identifier for request tracking across the execution.
        /// This identifier is used to correlate all log entries and operations related to a single batch job execution.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the correlation context was created.
        /// This represents the start time of the batch job execution or when the context was initialized.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the name of the batch job being executed.
        /// This property identifies which specific batch job is associated with this correlation context.
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContext"/> class.
        /// </summary>
        public CorrelationContext()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContext"/> class with specified values.
        /// </summary>
        /// <param name="correlationId">The unique correlation identifier.</param>
        /// <param name="jobName">The name of the batch job.</param>
        /// <exception cref="ArgumentNullException">Thrown when correlationId or jobName is null.</exception>
        public CorrelationContext(string correlationId, string jobName)
        {
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            JobName = jobName ?? throw new ArgumentNullException(nameof(jobName));
            Timestamp = DateTime.UtcNow;
        }
    }
}


// Key improvements made:
// 1. Initialized string properties with string.Empty to avoid nullable reference warnings (C# 8.0+)
// 2. Added null validation in the parameterized constructor to prevent null values from being assigned
// 3. Added XML documentation for the exception thrown by the parameterized constructor
// 4. Maintained all existing functionality without adding new features
// 5. Followed .NET naming conventions and best practices for defensive programming
