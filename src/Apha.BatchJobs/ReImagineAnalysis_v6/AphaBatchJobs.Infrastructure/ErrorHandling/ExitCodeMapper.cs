using AphaBatchJobs.Core.Enums;
using System;

namespace AphaBatchJobs.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Static class to map job execution status and exceptions to appropriate exit codes.
    /// </summary>
    public static class ExitCodeMapper
    {
        // Exit code constants for better maintainability and avoiding magic numbers
        private const int ExitCodeSuccess = 0;
        private const int ExitCodeFailed = 1;
        private const int ExitCodeCancelled = 2;
        private const int ExitCodePending = 3;
        private const int ExitCodeRunning = 4;
        private const int ExitCodeArgumentException = 10;
        private const int ExitCodeInvalidOperationException = 11;
        private const int ExitCodeTimeoutException = 12;
        private const int ExitCodeGeneralException = 99;

        /// <summary>
        /// Maps JobExecutionStatus to integer exit code.
        /// </summary>
        /// <param name="status">The job execution status.</param>
        /// <returns>Exit code: 0 for Completed, 1 for Failed, 2 for Cancelled, 3 for Pending, 4 for Running.</returns>
        public static int MapToExitCode(JobExecutionStatus status)
        {
            return status switch
            {
                JobExecutionStatus.Completed => ExitCodeSuccess,
                JobExecutionStatus.Failed => ExitCodeFailed,
                JobExecutionStatus.Cancelled => ExitCodeCancelled,
                JobExecutionStatus.Pending => ExitCodePending,
                JobExecutionStatus.Running => ExitCodeRunning,
                _ => ExitCodeFailed
            };
        }

        /// <summary>
        /// Maps Exception to integer exit code.
        /// </summary>
        /// <param name="exception">The exception to map.</param>
        /// <returns>Exit code: 10 for ArgumentException, 11 for InvalidOperationException, 12 for TimeoutException, 99 for general exceptions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when exception parameter is null.</exception>
        public static int MapExceptionToExitCode(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return exception switch
            {
                ArgumentException => ExitCodeArgumentException,
                InvalidOperationException => ExitCodeInvalidOperationException,
                TimeoutException => ExitCodeTimeoutException,
                _ => ExitCodeGeneralException
            };
        }

        /// <summary>
        /// Gets the description for a given exit code.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <returns>Description of the exit code.</returns>
        public static string GetExitCodeDescription(int exitCode)
        {
            return exitCode switch
            {
                ExitCodeSuccess => "Job completed successfully",
                ExitCodeFailed => "Job failed",
                ExitCodeCancelled => "Job cancelled",
                ExitCodePending => "Job pending",
                ExitCodeRunning => "Job running",
                ExitCodeArgumentException => "Argument exception",
                ExitCodeInvalidOperationException => "Invalid operation exception",
                ExitCodeTimeoutException => "Timeout exception",
                ExitCodeGeneralException => "General exception",
                _ => "Unknown exit code"
            };
        }
    }
}


// Key improvements made:
// 1. Introduced private constants for exit codes to eliminate magic numbers and improve maintainability
// 2. Added null check with ArgumentNullException in MapExceptionToExitCode for defensive programming
// 3. Added XML documentation for the ArgumentNullException
// 4. Replaced hardcoded numbers with named constants throughout all methods
// 5. Maintained all existing functionality without adding new features
// 6. Improved code readability and maintainability following .NET best practices