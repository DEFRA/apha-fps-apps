// ============================================================================
// File: JobExecutionContext.cs
// Description: Model class representing job execution context with correlation id, 
//              job parameters, and execution metadata for tracking and logging
// ============================================================================

using AphaBatchJobsFoundation.Core.Enums;

namespace AphaBatchJobsFoundation.Core.Models
{
    /// <summary>
    /// Represents the execution context for a batch job in the Apha BatchJobs system.
    /// Contains all necessary metadata and parameters required for job execution,
    /// tracking, and distributed logging across the job lifecycle.
    /// </summary>
    public class JobExecutionContext
    {
        /// <summary>
        /// Gets or sets the correlation identifier for distributed tracing and logging.
        /// This unique identifier is used to correlate log entries across the entire
        /// job execution lifecycle and related operations.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the job being executed.
        /// This should match the registered job name in the job orchestrator.
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of job execution (Scheduled or Adhoc).
        /// Used to distinguish between jobs running on a schedule versus
        /// those triggered manually or on-demand.
        /// </summary>
        public JobType JobType { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of job execution parameters.
        /// Contains key-value pairs of parameters passed from CLI arguments
        /// or scheduler configuration. Values are stored as objects to support
        /// various parameter types.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the timestamp when the job execution started.
        /// Used for duration calculation, performance monitoring, and audit logging.
        /// </summary>
        public DateTime StartedAt { get; set; }
    }
}


// Changes made:
// 1. Updated Dictionary initialization from `new Dictionary<string, object>()` to `new()` 
//    - Uses C# 9.0+ target-typed new expression for cleaner, more concise code
//    - Reduces redundancy while maintaining type safety
//    - This is a modern C# best practice for object initialization