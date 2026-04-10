using System;
using System.Collections.Generic;
using AphaBatchJobs.Core.Enums;

namespace AphaBatchJobs.Core.Models
{
    /// <summary>
    /// Represents the execution context for a batch job in the Apha BatchJobs orchestration system.
    /// This model encapsulates all necessary information required to track and execute a job,
    /// including correlation tracking, job identification, type classification, runtime parameters,
    /// and execution timing information.
    /// </summary>
    public class JobExecutionContext
    {
        /// <summary>
        /// Gets or sets the unique correlation identifier for tracking job execution across logs and systems.
        /// This identifier is used to correlate all log entries, database operations, and external service calls
        /// that occur during the execution of a single job instance.
        /// </summary>
        /// <remarks>
        /// The correlation ID should be generated at the start of job execution and propagated
        /// throughout the entire execution lifecycle for comprehensive traceability.
        /// </remarks>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the job being executed.
        /// This name should uniquely identify the job within the Apha BatchJobs system
        /// and is used for logging, monitoring, and job routing purposes.
        /// </summary>
        /// <remarks>
        /// Job names should follow Apha naming conventions and be descriptive enough
        /// to identify the job's purpose without requiring additional context.
        /// </remarks>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the job being executed.
        /// Indicates whether the job is a scheduled job (triggered by the scheduler)
        /// or an ad hoc job (triggered manually via CLI or external trigger).
        /// </summary>
        /// <remarks>
        /// The job type determines the execution path and may influence logging,
        /// error handling, and retry behavior.
        /// </remarks>
        public JobType JobType { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of parameters for job execution.
        /// Contains key-value pairs representing configuration and runtime parameters
        /// required by the job during execution.
        /// </summary>
        /// <remarks>
        /// Parameters can include configuration values, input data references, feature flags,
        /// or any other contextual information needed by the job implementation.
        /// The dictionary is initialized to prevent null reference exceptions.
        /// </remarks>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the timestamp when the job execution started.
        /// This value is used for execution duration calculation, timeout enforcement,
        /// and audit logging purposes.
        /// </summary>
        /// <remarks>
        /// The timestamp should be set at the beginning of job execution using UTC time
        /// to ensure consistency across different time zones and deployment environments.
        /// </remarks>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        // Removed parameterless constructor as property initializers handle default values
        // This is more idiomatic in modern C# and reduces code duplication
    }
}


**Key improvements made:**

1. **Property Initializers**: Moved initialization logic from constructor to property initializers (C# 6.0+ feature). This is more idiomatic and concise in modern .NET.

2. **Non-nullable String Initialization**: Initialized `CorrelationId` and `JobName` to `string.Empty` to avoid potential null reference warnings/errors, especially important for nullable reference types enabled projects.

3. **Removed Redundant Constructor**: The parameterless constructor is no longer needed since all initialization is handled by property initializers, reducing code duplication and maintenance overhead.

4. **Maintained Immutability Considerations**: While the properties remain mutable (as per original design), the initialization ensures the object is always in a valid state upon creation.

5. **UTC Time Consistency**: Kept `DateTime.UtcNow` for `StartedAt` to ensure timezone-independent timestamps, which is critical for distributed systems and AWS deployments across regions.