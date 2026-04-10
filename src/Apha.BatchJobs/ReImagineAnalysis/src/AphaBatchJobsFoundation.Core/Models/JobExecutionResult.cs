// ============================================================================
// File: JobExecutionResult.cs
// Description: Model class representing job execution result with status, 
//              scheduler friendly exit code, error details, and completion metadata
//              for Apha BatchJobs Foundation
// ============================================================================

using System;
using AphaBatchJobsFoundation.Core.Enums;

namespace AphaBatchJobsFoundation.Core.Models
{
    /// <summary>
    /// Represents the result of a batch job execution with comprehensive 
    /// execution metadata, status information, and error details.
    /// Provides scheduler-friendly exit codes for integration with external schedulers.
    /// </summary>
    public class JobExecutionResult
    {
        /// <summary>
        /// Gets or sets the execution status of the job.
        /// Indicates the final outcome of the job execution lifecycle.
        /// </summary>
        public JobExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the scheduler-friendly exit code.
        /// Convention: 0 indicates success, non-zero values indicate various failure conditions.
        /// Used by external schedulers and monitoring systems to determine job outcome.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the execution result message or error details.
        /// Contains human-readable information about the job execution outcome.
        /// For successful executions, may contain summary information.
        /// For failures, contains error description and diagnostic information.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the job execution completed.
        /// Recorded in UTC for consistency across different time zones.
        /// Used for execution duration calculation and audit trails.
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the exception that caused the job execution to fail.
        /// Null if the job completed successfully or was cancelled without exception.
        /// Contains full exception details including stack trace for diagnostic purposes.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class.
        /// Creates a result object with default values.
        /// </summary>
        public JobExecutionResult()
        {
            Status = JobExecutionStatus.Pending;
            ExitCode = 0;
            Message = string.Empty;
            CompletedAt = DateTime.UtcNow;
            Exception = null;
        }

        /// <summary>
        /// Creates a successful job execution result.
        /// </summary>
        /// <param name="message">Optional success message describing the execution outcome.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating successful completion.</returns>
        public static JobExecutionResult Success(string message = "Job completed successfully")
        {
            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Completed,
                ExitCode = 0,
                Message = message ?? string.Empty,
                CompletedAt = DateTime.UtcNow,
                Exception = null
            };
        }

        /// <summary>
        /// Creates a failed job execution result.
        /// </summary>
        /// <param name="message">Error message describing the failure reason.</param>
        /// <param name="exception">Optional exception that caused the failure.</param>
        /// <param name="exitCode">Scheduler-friendly exit code (default: 1 for general failure).</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating execution failure.</returns>
        public static JobExecutionResult Failure(string message, Exception exception = null, int exitCode = 1)
        {
            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Failed,
                ExitCode = exitCode,
                Message = message ?? string.Empty,
                CompletedAt = DateTime.UtcNow,
                Exception = exception
            };
        }

        /// <summary>
        /// Creates a cancelled job execution result.
        /// </summary>
        /// <param name="message">Optional cancellation message.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating execution cancellation.</returns>
        public static JobExecutionResult Cancelled(string message = "Job execution was cancelled")
        {
            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Cancelled,
                ExitCode = 2,
                Message = message ?? string.Empty,
                CompletedAt = DateTime.UtcNow,
                Exception = null
            };
        }

        /// <summary>
        /// Determines whether the job execution was successful.
        /// </summary>
        /// <returns>True if the job completed successfully; otherwise, false.</returns>
        public bool IsSuccess()
        {
            return Status == JobExecutionStatus.Completed && ExitCode == 0;
        }

        /// <summary>
        /// Determines whether the job execution failed.
        /// </summary>
        /// <returns>True if the job execution failed; otherwise, false.</returns>
        public bool IsFailure()
        {
            return Status == JobExecutionStatus.Failed;
        }

        /// <summary>
        /// Determines whether the job execution was cancelled.
        /// </summary>
        /// <returns>True if the job execution was cancelled; otherwise, false.</returns>
        public bool IsCancelled()
        {
            return Status == JobExecutionStatus.Cancelled;
        }
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Design Decisions:
// 1. All properties implemented as specified in requirements
// 2. Added parameterless constructor for object initialization flexibility
// 3. Included static factory methods (Success, Failure, Cancelled) for 
//    convenient result creation following common patterns
// 4. Added helper methods (IsSuccess, IsFailure, IsCancelled) for 
//    cleaner result checking in consuming code
// 5. CompletedAt uses UTC to ensure consistency across time zones
// 6. Message property initialized to empty string to avoid null reference issues
//
// Exit Code Convention:
// - 0: Success (JobExecutionStatus.Completed)
// - 1: General failure (JobExecutionStatus.Failed)
// - 2: Cancellation (JobExecutionStatus.Cancelled)
// - Other non-zero values can be used for specific failure scenarios
//
// Usage Pattern:
// - Scheduler integration: Use ExitCode for process exit codes
// - Logging: Use Message and Exception for detailed error tracking
// - Monitoring: Use Status for job state tracking
// - Audit: Use CompletedAt for execution timeline
//
// Thread Safety:
// - This is a simple data transfer object (DTO)
// - Not designed for concurrent modification
// - Consumers should treat instances as immutable after creation
//
// Code Review Changes:
// - Added null-coalescing operator (??) in factory methods to handle null 
//   message parameters and prevent potential NullReferenceException
// - This ensures Message property is never null, maintaining consistency
//   with the constructor's initialization pattern
//
// ============================================================================