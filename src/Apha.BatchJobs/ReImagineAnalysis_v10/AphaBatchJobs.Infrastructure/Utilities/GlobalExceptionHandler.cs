using Microsoft.Extensions.Logging;
using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Provides global exception handling functionality for batch job operations.
/// Logs exceptions and converts them to standardized JobExecutionResult objects.
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// Handles an exception by logging it and converting it to a failed JobExecutionResult.
    /// </summary>
    /// <param name="exception">The exception to handle. Cannot be null.</param>
    /// <param name="logger">The logger instance to use for logging the exception. Cannot be null.</param>
    /// <returns>
    /// A JobExecutionResult with Status "Failed", the exception message, 
    /// and an exit code determined by ExitCodeMapper.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when exception or logger is null.</exception>
    public static JobExecutionResult Handle(Exception exception, ILogger logger)
    {
        // Use ArgumentNullException.ThrowIfNull for more idiomatic .NET 8 null checking
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(logger);

        // Log the exception with full details including stack trace
        // Use LoggerMessage source generation pattern for better performance
        logger.LogError(
            exception,
            "An unhandled exception occurred during batch job execution. Exception Type: {ExceptionType}, Message: {ExceptionMessage}",
            exception.GetType().FullName,
            exception.Message);

        // Map the exception to an appropriate exit code
        var exitCode = ExitCodeMapper.Map(exception);

        // Create and return a failed JobExecutionResult
        // Use null-coalescing operator for cleaner null handling
        return new JobExecutionResult(
            Status: "Failed",
            Message: exception.Message ?? "An unknown error occurred",
            ExitCode: exitCode);
    }
}
