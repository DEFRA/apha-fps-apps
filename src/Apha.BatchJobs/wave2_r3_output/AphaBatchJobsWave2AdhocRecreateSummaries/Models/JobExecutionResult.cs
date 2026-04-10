using System;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Models
{
    /// <summary>
    /// Represents the result of a job execution with status, message, and exit code.
    /// </summary>
    public sealed class JobExecutionResult
    {
        /// <summary>
        /// Gets the execution status of the job.
        /// </summary>
        public JobExecutionStatus Status { get; init; }

        /// <summary>
        /// Gets a descriptive message about the execution result.
        /// </summary>
        public string Message { get; init; }

        /// <summary>
        /// Gets the exit code (0=success, 1=failure, 2=timeout).
        /// </summary>
        public int ExitCode { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class.
        /// </summary>
        public JobExecutionResult()
        {
            Status = JobExecutionStatus.Failed;
            Message = string.Empty;
            ExitCode = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class with specified values.
        /// </summary>
        /// <param name="status">The execution status.</param>
        /// <param name="message">The descriptive message.</param>
        /// <param name="exitCode">The exit code.</param>
        public JobExecutionResult(JobExecutionStatus status, string message, int exitCode)
        {
            Status = status;
            Message = message ?? string.Empty;
            ExitCode = exitCode;
        }

        /// <summary>
        /// Creates a successful job execution result.
        /// </summary>
        /// <param name="message">Optional success message.</param>
        /// <returns>A JobExecutionResult indicating success.</returns>
        public static JobExecutionResult Success(string message = "Job completed successfully")
        {
            return new JobExecutionResult(JobExecutionStatus.Success, message, 0);
        }

        /// <summary>
        /// Creates a failed job execution result.
        /// </summary>
        /// <param name="message">The failure message.</param>
        /// <returns>A JobExecutionResult indicating failure.</returns>
        public static JobExecutionResult Failure(string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
            return new JobExecutionResult(JobExecutionStatus.Failed, message, 1);
        }

        /// <summary>
        /// Creates a timeout job execution result.
        /// </summary>
        /// <param name="message">The timeout message.</param>
        /// <returns>A JobExecutionResult indicating timeout.</returns>
        public static JobExecutionResult Timeout(string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
            return new JobExecutionResult(JobExecutionStatus.Timeout, message, 2);
        }
    }

    /// <summary>
    /// Defines the possible execution statuses for a job.
    /// </summary>
    public enum JobExecutionStatus
    {
        /// <summary>
        /// Job completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Job failed during execution.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// Job execution timed out.
        /// </summary>
        Timeout = 2
    }
}


// Key improvements made:
// 1. Made the class 'sealed' since it's not designed for inheritance and this provides better performance
// 2. Changed properties from 'set' to 'init' to make the object immutable after construction, following .NET 8 best practices
// 3. Added ArgumentException.ThrowIfNullOrWhiteSpace validation in Failure() and Timeout() methods to ensure messages are meaningful
// 4. Maintained all existing functionality without adding new features
// 5. Immutability is important for thread-safety in Quartz job execution contexts where results may be accessed concurrently