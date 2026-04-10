using AphaBatchJobs.Core.Enums;

namespace AphaBatchJobs.Core.Models
{
    /// <summary>
    /// Represents the result of a batch job execution.
    /// Contains comprehensive information about the job execution outcome including status,
    /// error details, performance metrics, and completion timestamp.
    /// This model is used to communicate job execution results between the orchestration layer
    /// and the scheduler/monitoring systems.
    /// </summary>
    public sealed class JobExecutionResult
    {
        /// <summary>
        /// Gets or sets the final execution status of the job.
        /// Indicates whether the job completed successfully, failed, was cancelled, or is still in progress.
        /// </summary>
        public JobExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the error message if the job execution failed.
        /// Contains a high-level description of the error that occurred during job execution.
        /// This property is null or empty when the job completes successfully.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets detailed error information including stack trace and inner exception details.
        /// Provides comprehensive diagnostic information for troubleshooting failed job executions.
        /// This property is null or empty when the job completes successfully.
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets the total execution time in milliseconds.
        /// Represents the duration from job start to completion, used for performance monitoring
        /// and identifying long-running or problematic jobs.
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the job execution completed.
        /// Recorded in UTC to ensure consistency across different time zones and deployment environments.
        /// This timestamp is set regardless of whether the job succeeded or failed.
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class.
        /// Creates a result object with default values.
        /// </summary>
        public JobExecutionResult()
        {
            Status = JobExecutionStatus.Pending;
            ExecutionTimeMs = 0;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class with specified status.
        /// </summary>
        /// <param name="status">The execution status of the job.</param>
        public JobExecutionResult(JobExecutionStatus status)
        {
            Status = status;
            ExecutionTimeMs = 0;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a successful job execution result.
        /// </summary>
        /// <param name="executionTimeMs">The execution time in milliseconds.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating successful completion.</returns>
        public static JobExecutionResult Success(long executionTimeMs)
        {
            // Validate execution time to prevent negative values
            ArgumentOutOfRangeException.ThrowIfNegative(executionTimeMs, nameof(executionTimeMs));

            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Success,
                ExecutionTimeMs = executionTimeMs,
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failed job execution result with error information.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <param name="errorDetails">Detailed error information including stack trace.</param>
        /// <param name="executionTimeMs">The execution time in milliseconds before failure.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating failure with error details.</returns>
        public static JobExecutionResult Failure(string errorMessage, string? errorDetails, long executionTimeMs)
        {
            // Validate required parameters
            ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));
            ArgumentOutOfRangeException.ThrowIfNegative(executionTimeMs, nameof(executionTimeMs));

            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Failed,
                ErrorMessage = errorMessage,
                ErrorDetails = errorDetails,
                ExecutionTimeMs = executionTimeMs,
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a cancelled job execution result.
        /// </summary>
        /// <param name="executionTimeMs">The execution time in milliseconds before cancellation.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating cancellation.</returns>
        public static JobExecutionResult Cancelled(long executionTimeMs)
        {
            // Validate execution time to prevent negative values
            ArgumentOutOfRangeException.ThrowIfNegative(executionTimeMs, nameof(executionTimeMs));

            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Cancelled,
                ExecutionTimeMs = executionTimeMs,
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Determines whether the job execution was successful.
        /// </summary>
        /// <returns>True if the job completed successfully; otherwise, false.</returns>
        public bool IsSuccess() => Status == JobExecutionStatus.Success;

        /// <summary>
        /// Determines whether the job execution failed.
        /// </summary>
        /// <returns>True if the job execution failed; otherwise, false.</returns>
        public bool IsFailure() => Status == JobExecutionStatus.Failed;

        /// <summary>
        /// Determines whether the job execution was cancelled.
        /// </summary>
        /// <returns>True if the job execution was cancelled; otherwise, false.</returns>
        public bool IsCancelled() => Status == JobExecutionStatus.Cancelled;

        /// <summary>
        /// Determines whether the job execution has completed (successfully, failed, or cancelled).
        /// </summary>
        /// <returns>True if the job has reached a terminal state; otherwise, false.</returns>
        public bool IsCompleted() => Status >= JobExecutionStatus.Success;
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to prevent inheritance, improving performance and clarity of intent
// 2. Added parameter validation in factory methods using modern .NET ArgumentException.ThrowIfNullOrWhiteSpace and ArgumentOutOfRangeException.ThrowIfNegative
// 3. Validates executionTimeMs to prevent negative values which would be logically incorrect
// 4. Validates errorMessage in Failure method to ensure meaningful error information is provided
// 5. These changes improve robustness and fail-fast behavior while maintaining existing functionality