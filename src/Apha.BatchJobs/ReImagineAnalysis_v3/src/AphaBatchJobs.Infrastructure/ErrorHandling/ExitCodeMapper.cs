using AphaBatchJobs.Core.Enums;
using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Infrastructure.ErrorHandling;

/// <summary>
/// Service class responsible for mapping job execution results and exceptions to scheduler-friendly exit codes.
/// This mapper enables external schedulers and monitoring systems to understand job execution outcomes
/// through standardized exit codes and provides human-readable descriptions for logging and reporting.
/// </summary>
/// <remarks>
/// The mapper follows Unix exit code conventions where 0 indicates success and non-zero values indicate failures.
/// Exit codes are designed to help schedulers make intelligent decisions about retry logic, alerting, and error handling.
/// </remarks>
public sealed class ExitCodeMapper
{
    /// <summary>
    /// Maps a JobExecutionResult to an appropriate scheduler-friendly exit code.
    /// The exit code is determined based on the execution status and error details in the result.
    /// </summary>
    /// <param name="result">The job execution result to map to an exit code.</param>
    /// <returns>An integer exit code representing the job execution outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    public int MapFromResult(JobExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status switch
        {
            JobExecutionStatus.Success => ExitCodeConstants.Success,
            JobExecutionStatus.Failed => DetermineFailureExitCode(result),
            JobExecutionStatus.Cancelled => ExitCodeConstants.GeneralError,
            JobExecutionStatus.Pending => ExitCodeConstants.GeneralError,
            JobExecutionStatus.Running => ExitCodeConstants.GeneralError,
            _ => ExitCodeConstants.GeneralError
        };
    }

    /// <summary>
    /// Maps an exception to an appropriate scheduler-friendly exit code.
    /// Different exception types are mapped to specific exit codes to enable intelligent error handling by schedulers.
    /// </summary>
    /// <param name="exception">The exception to map to an exit code.</param>
    /// <returns>An integer exit code representing the exception type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null.</exception>
    public int MapFromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TimeoutException => ExitCodeConstants.TimeoutError,
            InvalidOperationException invalidOp when IsJobNotFoundException(invalidOp) => ExitCodeConstants.JobNotFound,
            InvalidOperationException => ExitCodeConstants.ConfigurationError,
            ArgumentException => ExitCodeConstants.ConfigurationError,
            ArgumentNullException => ExitCodeConstants.ConfigurationError,
            Npgsql.NpgsqlException => ExitCodeConstants.DatabaseError,
            System.Data.Common.DbException => ExitCodeConstants.DatabaseError,
            OperationCanceledException => ExitCodeConstants.GeneralError,
            _ => ExitCodeConstants.GeneralError
        };
    }

    /// <summary>
    /// Returns a human-readable description of the specified exit code.
    /// This description is useful for logging, reporting, and troubleshooting job execution issues.
    /// </summary>
    /// <param name="exitCode">The exit code to describe.</param>
    /// <returns>A string containing a human-readable description of the exit code.</returns>
    public string GetExitCodeDescription(int exitCode)
    {
        return exitCode switch
        {
            ExitCodeConstants.Success => "Job completed successfully without errors",
            ExitCodeConstants.GeneralError => "Job failed due to an unexpected error",
            ExitCodeConstants.ConfigurationError => "Job failed due to invalid or missing configuration",
            ExitCodeConstants.DatabaseError => "Job failed due to database connection or operation error",
            ExitCodeConstants.JobNotFound => "Requested job was not found in the system",
            ExitCodeConstants.TimeoutError => "Job execution exceeded maximum allowed time",
            _ => $"Unknown exit code: {exitCode}"
        };
    }

    /// <summary>
    /// Determines the appropriate exit code for a failed job execution by analyzing error details.
    /// This method examines the error message and details to identify specific failure categories.
    /// </summary>
    /// <param name="result">The failed job execution result.</param>
    /// <returns>An integer exit code representing the specific failure type.</returns>
    private int DetermineFailureExitCode(JobExecutionResult result)
    {
        if (string.IsNullOrWhiteSpace(result.ErrorMessage) && string.IsNullOrWhiteSpace(result.ErrorDetails))
        {
            return ExitCodeConstants.GeneralError;
        }

        // Use StringComparison.OrdinalIgnoreCase for better performance and culture-invariant comparison
        var errorText = $"{result.ErrorMessage} {result.ErrorDetails}";

        if (ContainsAny(errorText, "timeout", "timed out"))
        {
            return ExitCodeConstants.TimeoutError;
        }

        if (ContainsAny(errorText, "job not found", "jobnotfound"))
        {
            return ExitCodeConstants.JobNotFound;
        }

        if (ContainsAny(errorText, "database", "connection", "npgsql", "postgres", "sql"))
        {
            return ExitCodeConstants.DatabaseError;
        }

        if (ContainsAny(errorText, "configuration", "config", "setting", "invalid argument", "missing required"))
        {
            return ExitCodeConstants.ConfigurationError;
        }

        return ExitCodeConstants.GeneralError;
    }

    /// <summary>
    /// Determines if an InvalidOperationException represents a job not found error.
    /// This helper method checks the exception message for job not found indicators.
    /// </summary>
    /// <param name="exception">The InvalidOperationException to check.</param>
    /// <returns>True if the exception represents a job not found error; otherwise, false.</returns>
    private static bool IsJobNotFoundException(InvalidOperationException exception)
    {
        if (string.IsNullOrWhiteSpace(exception.Message))
        {
            return false;
        }

        return ContainsAny(exception.Message, "job not found", "jobnotfound", "no job found", "job does not exist");
    }

    /// <summary>
    /// Checks if the text contains any of the specified keywords using case-insensitive, culture-invariant comparison.
    /// </summary>
    /// <param name="text">The text to search in.</param>
    /// <param name="keywords">The keywords to search for.</param>
    /// <returns>True if any keyword is found; otherwise, false.</returns>
    private static bool ContainsAny(string text, params string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}


// Key improvements made:
// 1. Removed redundant 'nameof(result)' and 'nameof(exception)' from ArgumentNullException.ThrowIfNull calls (parameter name is inferred automatically)
// 2. Replaced ToLowerInvariant() + Contains() with Contains(StringComparison.OrdinalIgnoreCase) for better performance and culture-invariant comparison
// 3. Made IsJobNotFoundException static since it doesn't use instance state
// 4. Extracted repeated Contains logic into a reusable ContainsAny helper method to reduce code duplication and improve maintainability
// 5. Removed unnecessary string concatenation and ToLowerInvariant() call in DetermineFailureExitCode for better performance
// 6. Made ContainsAny static and added null/whitespace check for robustness