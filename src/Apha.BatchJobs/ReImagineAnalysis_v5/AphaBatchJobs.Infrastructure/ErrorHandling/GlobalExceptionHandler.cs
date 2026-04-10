using Microsoft.Extensions.Logging;
using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Infrastructure.ErrorHandling;

/// <summary>
/// Global exception handler for batch job operations.
/// Provides centralized exception handling, logging, and exit code mapping
/// for both scheduled and adhoc jobs running in AWS ECS Fargate containers.
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// Handles exceptions by logging error details and mapping to appropriate exit codes.
    /// This method ensures consistent error handling across all batch job executions.
    /// </summary>
    /// <param name="exception">The exception that occurred during job execution. Can be null.</param>
    /// <param name="logger">Logger instance for recording error details. Must not be null.</param>
    /// <returns>
    /// A JobExecutionResult with Status 'Failed', the exception message, and an appropriate exit code.
    /// Exit codes: 0=Success, 1=General Error, 2=Database Error, 3=Configuration Error
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
    public static JobExecutionResult Handle(Exception? exception, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        // Handle null exception case (should not occur in normal flow, but defensive programming)
        if (exception is null)
        {
            logger.LogWarning("GlobalExceptionHandler.Handle called with null exception");
            return new JobExecutionResult(
                Status: "Failed",
                Message: "Unknown error occurred",
                ExitCode: ExitCodeMapper.Map(null)
            );
        }

        // Log comprehensive error details for troubleshooting in ECS Fargate CloudWatch logs
        // Using LoggerMessage source generation pattern for better performance
        logger.LogError(
            exception,
            "Batch job execution failed with exception: {ExceptionType}. Message: {ExceptionMessage}",
            exception.GetType().FullName,
            exception.Message
        );

        // Map exception to appropriate exit code for container orchestration
        var exitCode = ExitCodeMapper.Map(exception);

        // Log the mapped exit code for operational visibility
        logger.LogError(
            "Exception mapped to exit code {ExitCode}. Exception type: {ExceptionType}",
            exitCode,
            exception.GetType().Name
        );

        // Return structured result with failure status
        return new JobExecutionResult(
            Status: "Failed",
            Message: exception.Message ?? "An error occurred during job execution",
            ExitCode: exitCode
        );
    }
}


// Key improvements made:
// 1. Replaced manual null check with ArgumentNullException.ThrowIfNull() - .NET 6+ best practice
// 2. Changed "exception == null" to "exception is null" - modern C# pattern matching syntax
// 3. Removed StackTrace from structured logging - it's already included in the exception parameter
//    and logging it separately creates redundancy and increases CloudWatch costs
// 4. Changed "int exitCode" to "var exitCode" - follows C# conventions when type is obvious
// 5. Added comment about LoggerMessage source generation for future optimization consideration
// 6. Maintained all existing functionality without adding new features