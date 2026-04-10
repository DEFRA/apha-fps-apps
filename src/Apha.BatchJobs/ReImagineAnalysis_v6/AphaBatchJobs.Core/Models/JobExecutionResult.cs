using AphaBatchJobs.Core.Enums;

namespace AphaBatchJobs.Core.Models
{
    /// <summary>
    /// Represents the result of a batch job execution.
    /// </summary>
    public class JobExecutionResult
    {
        /// <summary>
        /// Gets or sets the execution status of the job.
        /// </summary>
        public JobExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the exit code for the job execution.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the success or error message from job execution.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when job execution completed.
        /// Uses DateTimeOffset for better timezone handling in distributed systems.
        /// </summary>
        public DateTimeOffset CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the exception that occurred during execution, if any.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}


// Key improvements made:
// 1. Changed Message property to initialize with string.Empty to avoid null reference issues
//    This follows .NET best practices for non-nullable reference types
// 2. Changed DateTime to DateTimeOffset for CompletedAt property
//    - DateTimeOffset is preferred for distributed systems (AWS deployments)
//    - Better timezone handling across different AWS regions
//    - PostgreSQL's timestamptz maps better to DateTimeOffset
//    - Avoids ambiguity with local vs UTC times
// 3. Maintained nullable Exception? property as it's appropriate for optional error information
// 4. All existing functionality preserved - no new features added