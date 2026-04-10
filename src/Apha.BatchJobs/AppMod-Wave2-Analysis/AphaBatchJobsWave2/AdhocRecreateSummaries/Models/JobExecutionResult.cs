namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Models
{
    /// <summary>
    /// Represents the result of an adhoc job execution.
    /// Contains status information, detailed messages, and exit codes
    /// to communicate the outcome of job processing.
    /// </summary>
    /// <remarks>
    /// Exit codes follow the contract:
    /// - 0: Success - Job completed successfully
    /// - 1: Failure - Job failed due to an error
    /// - 2: Timeout - Job exceeded the allowed execution time
    /// </remarks>
    public sealed class JobExecutionResult
    {
        /// <summary>
        /// Gets the execution status of the job.
        /// </summary>
        /// <value>
        /// A string representing the status such as "Success", "Failure", or "Timeout".
        /// </value>
        public string Status { get; init; } = string.Empty;

        /// <summary>
        /// Gets the detailed message describing the execution result.
        /// </summary>
        /// <value>
        /// A string containing detailed information about the execution,
        /// including error messages, warnings, or success confirmations.
        /// </value>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Gets the exit code indicating the execution outcome.
        /// </summary>
        /// <value>
        /// An integer representing the exit code:
        /// 0 for success, 1 for failure, 2 for timeout.
        /// </value>
        public int ExitCode { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class.
        /// </summary>
        public JobExecutionResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionResult"/> class
        /// with specified status, message, and exit code.
        /// </summary>
        /// <param name="status">The execution status.</param>
        /// <param name="message">The detailed execution message.</param>
        /// <param name="exitCode">The exit code (0=success, 1=failure, 2=timeout).</param>
        public JobExecutionResult(string status, string message, int exitCode)
        {
            Status = status ?? string.Empty;
            Message = message ?? string.Empty;
            ExitCode = exitCode;
        }

        /// <summary>
        /// Creates a success result with exit code 0.
        /// </summary>
        /// <param name="message">Optional success message.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating success.</returns>
        public static JobExecutionResult Success(string message = "Job completed successfully")
        {
            return new JobExecutionResult("Success", message, 0);
        }

        /// <summary>
        /// Creates a failure result with exit code 1.
        /// </summary>
        /// <param name="message">The failure message describing the error.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating failure.</returns>
        public static JobExecutionResult Failure(string message)
        {
            return new JobExecutionResult("Failure", message ?? "Job execution failed", 1);
        }

        /// <summary>
        /// Creates a timeout result with exit code 2.
        /// </summary>
        /// <param name="message">Optional timeout message.</param>
        /// <returns>A <see cref="JobExecutionResult"/> indicating timeout.</returns>
        public static JobExecutionResult Timeout(string message = "Job execution timed out")
        {
            return new JobExecutionResult("Timeout", message, 2);
        }
    }
}


// Key improvements made:
// 1. Made class 'sealed' - This is a data model with no intended inheritance, sealing improves performance
// 2. Changed properties from 'set' to 'init' - Promotes immutability for result objects, aligning with .NET 8 best practices
// 3. Added null-coalescing operators in constructor and Failure method - Defensive programming to prevent null values
// 4. Changed XML doc comments from "Gets or sets" to "Gets" - Reflects the init-only nature of properties
// 5. Maintained all existing functionality without adding new features