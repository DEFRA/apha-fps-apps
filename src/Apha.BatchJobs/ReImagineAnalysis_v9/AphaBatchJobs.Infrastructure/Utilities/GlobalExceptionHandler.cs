using Microsoft.Extensions.Logging;
using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Static utility class for handling unhandled exceptions and converting them to JobExecutionResult.
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// Handles an unhandled exception by logging it and converting it to a failed JobExecutionResult.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="logger">The logger instance to log the exception.</param>
    /// <returns>A failed JobExecutionResult with status Failed, exception message, and mapped exit code.</returns>
    public static JobExecutionResult Handle(Exception exception, ILogger logger)
    {
        // Use ArgumentNullException.ThrowIfNull for .NET 8 idiomatic null checking
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(logger);

        // Log the exception with error level using structured logging
        // Include stack trace information for better debugging
        logger.LogError(
            exception,
            "Unhandled exception occurred: {ExceptionType} - {ExceptionMessage}",
            exception.GetType().FullName,
            exception.Message);

        // Map the exception to an exit code
        var exitCode = ExitCodeMapper.Map(exception);

        // Return a failed JobExecutionResult
        // Use GetMessage() helper to safely extract exception message with fallback
        return new JobExecutionResult(
            Status: "Failed",
            Message: GetSafeExceptionMessage(exception),
            ExitCode: exitCode);
    }

    /// <summary>
    /// Safely extracts exception message, handling potential null or empty messages.
    /// </summary>
    /// <param name="exception">The exception to extract message from.</param>
    /// <returns>A non-null exception message.</returns>
    private static string GetSafeExceptionMessage(Exception exception)
    {
        return !string.IsNullOrWhiteSpace(exception.Message)
            ? exception.Message
            : $"An error occurred: {exception.GetType().Name}";
    }
}


// Key improvements made:
// 1. Replaced manual null checks with ArgumentNullException.ThrowIfNull() - .NET 8 idiomatic approach
// 2. Added GetSafeExceptionMessage() helper method to handle edge cases where exception.Message might be null or empty
// 3. Improved code documentation and maintainability
// 4. Maintained all existing functionality without adding new features
// 5. Better defensive programming by ensuring Message is never null in the result