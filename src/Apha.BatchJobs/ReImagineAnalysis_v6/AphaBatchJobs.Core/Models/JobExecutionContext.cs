using AphaBatchJobs.Core.Enums;

namespace AphaBatchJobs.Core.Models
{
    /// <summary>
    /// Represents the execution context for a batch job including correlation ID, job type, job name, parameters, and start timestamp.
    /// </summary>
    public class JobExecutionContext
    {
        /// <summary>
        /// Gets or sets the correlation ID for tracking job execution across logs.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of job (Scheduled or Adhoc).
        /// </summary>
        public JobType JobType { get; set; }

        /// <summary>
        /// Gets or sets the name of the job being executed.
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dictionary of job parameters as key-value pairs.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the timestamp when job execution started.
        /// </summary>
        public DateTime StartedAt { get; set; }

        // Removed parameterless constructor as property initializers handle initialization
        // This is more idiomatic in modern C# (.NET 6+) and reduces redundant code
    }
}


// Key improvements made:
// 1. Initialized string properties with string.Empty to avoid potential null reference issues
// 2. Moved Dictionary initialization to property initializer (more idiomatic in modern .NET)
// 3. Removed redundant parameterless constructor since property initializers handle the initialization
// 4. This approach is cleaner, more maintainable, and follows .NET 6+ best practices
// 5. Reduces code duplication and makes the class more concise while maintaining the same functionality