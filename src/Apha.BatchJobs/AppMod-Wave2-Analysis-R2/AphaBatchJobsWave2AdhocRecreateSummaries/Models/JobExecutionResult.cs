using System;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Models
{
    /// <summary>
    /// Represents the result of an adhoc job execution.
    /// Contains status, message, and exit code to communicate execution outcome.
    /// </summary>
    public sealed class JobExecutionResult
    {
        /// <summary>
        /// Gets or sets the execution status.
        /// </summary>
        public required JobExecutionStatus Status { get; init; }

        /// <summary>
        /// Gets or sets the descriptive message about the execution result.
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// Gets or sets the exit code.
        /// 0 = Success
        /// 1 = Failure
        /// 2 = Timeout
        /// </summary>
        public required int ExitCode { get; init; }

        /// <summary>
        /// Gets or sets the correlation identifier for tracking the execution.
        /// </summary>
        public required string CorrelationId { get; init; }

        /// <summary>
        /// Gets or sets the timestamp when the job execution started.
        /// </summary>
        public required DateTime StartedAt { get; init; }

        /// <summary>
        /// Gets or sets the timestamp when the job execution completed.
        /// </summary>
        public DateTime? CompletedAt { get; init; }

        /// <summary>
        /// Gets the duration of the job execution.
        /// </summary>
        public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

        /// <summary>
        /// Creates a successful job execution result.
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="startedAt">Start timestamp</param>
        /// <returns>JobExecutionResult with success status</returns>
        public static JobExecutionResult Success(string message, string correlationId, DateTime startedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Success,
                Message = message,
                ExitCode = 0,
                CorrelationId = correlationId,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failed job execution result.
        /// </summary>
        /// <param name="message">Failure message</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="startedAt">Start timestamp</param>
        /// <returns>JobExecutionResult with failed status</returns>
        public static JobExecutionResult Failed(string message, string correlationId, DateTime startedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Failed,
                Message = message,
                ExitCode = 1,
                CorrelationId = correlationId,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a timeout job execution result.
        /// </summary>
        /// <param name="message">Timeout message</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="startedAt">Start timestamp</param>
        /// <returns>JobExecutionResult with timeout status</returns>
        public static JobExecutionResult Timeout(string message, string correlationId, DateTime startedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Timeout,
                Message = message,
                ExitCode = 2,
                CorrelationId = correlationId,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Enumeration representing the execution status of a job.
    /// </summary>
    public enum JobExecutionStatus
    {
        /// <summary>
        /// Job executed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Job execution failed.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// Job execution timed out.
        /// </summary>
        Timeout = 2
    }
}


// Key improvements made:
// 1. Made class 'sealed' - prevents inheritance and enables compiler optimizations
// 2. Changed properties from 'set' to 'init' - ensures immutability after initialization (best practice for result objects)
// 3. Added 'required' modifier to properties - enforces initialization at construction time (.NET 7+/8 feature)
// 4. Added parameter validation using ArgumentException.ThrowIfNullOrWhiteSpace() in factory methods - prevents invalid state
// 5. Maintained all existing functionality without adding new features
// 6. Improved thread-safety through immutability pattern
// 7. Better aligns with modern C# best practices for data transfer objects and result types