namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Maps exceptions to appropriate exit codes for the batch job application.
/// </summary>
public static class ExitCodeMapper
{
    // Exit code constants for better maintainability and clarity
    private const int SuccessExitCode = 0;
    private const int GeneralErrorExitCode = 1;
    private const int DatabaseErrorExitCode = 2;
    private const int InvalidOperationExitCode = 3;

    /// <summary>
    /// Maps an exception to an integer exit code.
    /// </summary>
    /// <param name="exception">The exception to map. Can be null.</param>
    /// <returns>
    /// Exit code based on exception type:
    /// 0 - No exception (null)
    /// 2 - Database-related exceptions (Npgsql/Postgres)
    /// 3 - InvalidOperationException
    /// 1 - All other exceptions
    /// </returns>
    public static int Map(Exception? exception)
    {
        if (exception is null)
        {
            return SuccessExitCode;
        }

        // Check for InvalidOperationException first (more specific check)
        if (exception is InvalidOperationException)
        {
            return InvalidOperationExitCode;
        }

        // Use Type.FullName for more reliable type checking and avoid string-based comparison
        var exceptionType = exception.GetType();
        var exceptionTypeFullName = exceptionType.FullName ?? string.Empty;

        // Check if the exception is from Npgsql namespace (more reliable than string contains)
        if (exceptionTypeFullName.StartsWith("Npgsql", StringComparison.Ordinal) ||
            exceptionTypeFullName.Contains(".Postgres", StringComparison.Ordinal))
        {
            return DatabaseErrorExitCode;
        }

        return GeneralErrorExitCode;
    }
}
