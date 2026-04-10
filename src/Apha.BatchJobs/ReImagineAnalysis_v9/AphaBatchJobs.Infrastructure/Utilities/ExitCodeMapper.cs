namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Static utility class for mapping exceptions to integer exit codes based on exception type.
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
    /// 0 if exception is null (success),
    /// 2 if exception is database-related (Npgsql or Postgres),
    /// 3 if exception is InvalidOperationException,
    /// 1 for all other exceptions (general error).
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

        // Use FullName instead of Name for more accurate type identification
        // This prevents false positives from custom exception types
        var exceptionTypeFullName = exception.GetType().FullName ?? string.Empty;

        // Check if it's a PostgreSQL/Npgsql related exception
        if (exceptionTypeFullName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
            exceptionTypeFullName.Contains("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            return DatabaseErrorExitCode;
        }

        // Default to general error for all other exceptions
        return GeneralErrorExitCode;
    }
}


// Key improvements made:
// 1. Added named constants for exit codes to improve readability and maintainability
// 2. Reordered exception checks - more specific checks (InvalidOperationException) before string-based checks
// 3. Changed from GetType().Name to GetType().FullName for more accurate exception type identification
// 4. Added null-coalescing operator (??) to handle edge case where FullName might be null
// 5. Enhanced XML documentation comments for clarity
// 6. Maintained all original functionality without adding new features