using Microsoft.Extensions.Logging;
using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Centralized exception handling for job execution.
/// Provides consistent error logging and result mapping across all batch jobs.
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// Handles exceptions that occur during job execution by logging them and converting to JobExecutionResult.
    /// </summary>
    /// <param name="exception">The exception that occurred during job execution. Cannot be null.</param>
    /// <param name="logger">The logger instance for recording exception details. Cannot be null.</param>
    /// <returns>
    /// A JobExecutionResult with Status='Failed', the exception message, and an exit code determined by ExitCodeMapper.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when exception or logger is null.</exception>
    public static JobExecutionResult Handle(Exception exception, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        // Log the exception with full details including stack trace
        // Use structured logging with proper log level and include inner exception details
        logger.LogError(
            exception,
            "Job execution failed with exception: {ExceptionType}. Message: {ExceptionMessage}",
            exception.GetType().Name,
            exception.Message
        );

        // Map the exception to an appropriate exit code
        int exitCode = ExitCodeMapper.Map(exception);

        // Create and return a failed JobExecutionResult
        // Use target-typed new expression for cleaner code (C# 9.0+)
        return new JobExecutionResult(
            Status: "Failed",
            Message: exception.Message,
            ExitCode: exitCode
        );
    }
}


// Changes made:
// 1. Removed duplicate 'using AphaBatchJobs.Infrastructure.Utilities;' - namespace should not import itself
// 2. Code structure and logic remain unchanged as they follow .NET best practices
// 3. ArgumentNullException.ThrowIfNull is appropriate for .NET 6+ (assuming Dotnet10 refers to .NET 10)
// 4. Structured logging is correctly implemented with exception as first parameter
// 5. Record type instantiation syntax is correct for C# 9.0+
// 6. Exception handling follows fail-fast principle with proper null checks