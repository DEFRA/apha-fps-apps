using AphaBatchJobs.Core.Enums;

namespace AphaBatchJobs.Core.Models
{
    /// <summary>
    /// Represents the execution context for a batch job in the Apha BatchJobs system.
    /// This model encapsulates all contextual information required during job execution,
    /// including correlation tracking, job identification, parameters, and timing information.
    /// </summary>
    public class JobExecutionContext
    {
        /// <summary>
        /// Gets or sets the unique correlation identifier for tracking job execution across logs and systems.
        /// This identifier is used to correlate all log entries, database operations, and external service calls
        /// that occur during the execution of a single job instance.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the job being executed.
        /// This should be a unique, descriptive identifier for the job type (e.g., "DailyReportGeneration", "DataSync").
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of job being executed.
        /// Indicates whether the job is a scheduled job (runs automatically at predefined intervals)
        /// or an ad hoc job (triggered manually via CLI or API).
        /// </summary>
        public JobType JobType { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of parameters passed to the job for execution.
        /// Parameters can include configuration values, input data, or runtime options specific to the job.
        /// The dictionary allows flexible parameter passing with string keys and object values.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the timestamp when the job execution started.
        /// This value is set when the job begins execution and is used for duration calculations,
        /// timeout enforcement, and audit logging.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionContext"/> class.
        /// Creates an empty context with default values for all properties.
        /// </summary>
        public JobExecutionContext()
        {
            // Property initializers handle default values, no need to reassign
            // CorrelationId, JobName, and Parameters are already initialized above
            // JobType defaults to the first enum value (Scheduled = 0)
            StartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionContext"/> class with specified values.
        /// </summary>
        /// <param name="correlationId">The unique correlation identifier for the job execution.</param>
        /// <param name="jobName">The name of the job being executed.</param>
        /// <param name="jobType">The type of job (Scheduled or Adhoc).</param>
        /// <param name="parameters">Optional dictionary of parameters for the job execution.</param>
        /// <exception cref="ArgumentNullException">Thrown when correlationId or jobName is null.</exception>
        public JobExecutionContext(string correlationId, string jobName, JobType jobType, Dictionary<string, object>? parameters = null)
        {
            // Use ArgumentNullException.ThrowIfNull for .NET 6+ (more idiomatic)
            // If targeting older .NET versions, keep the original pattern
            ArgumentNullException.ThrowIfNull(correlationId);
            ArgumentNullException.ThrowIfNull(jobName);
            
            CorrelationId = correlationId;
            JobName = jobName;
            JobType = jobType;
            Parameters = parameters ?? new Dictionary<string, object>();
            StartedAt = DateTime.UtcNow;
        }
    }
}


**Key improvements made:**

1. **Modern collection initialization**: Changed `new Dictionary<string, object>()` to `new()` for cleaner syntax (C# 9+)

2. **Removed redundant assignments in default constructor**: Property initializers already set default values for `CorrelationId`, `JobName`, and `Parameters`, so no need to reassign them in the constructor

3. **Modern null checking**: Replaced the inline null check pattern with `ArgumentNullException.ThrowIfNull()` which is more idiomatic for .NET 6+ and provides better performance

4. **Added XML documentation for exception**: Added `<exception>` tag to document the `ArgumentNullException` that can be thrown

5. **Consistent formatting**: Maintained consistent code style throughout

**Note**: If your project targets .NET versions earlier than .NET 6, you should keep the original null-checking pattern:

CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));