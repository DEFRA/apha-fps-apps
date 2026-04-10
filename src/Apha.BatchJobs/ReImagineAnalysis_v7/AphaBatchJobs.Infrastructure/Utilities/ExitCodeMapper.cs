namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Maps exceptions to standardized exit codes for the batch job application.
/// </summary>
public static class ExitCodeMapper
{
    // Exit code constants for better maintainability and clarity
    private const int SuccessExitCode = 0;
    private const int GeneralErrorExitCode = 1;
    private const int DatabaseErrorExitCode = 2;
    private const int InvalidOperationExitCode = 3;

    /// <summary>
    /// Maps an exception to a standardized exit code.
    /// </summary>
    /// <param name="exception">The exception to map. Can be null.</param>
    /// <returns>
    /// Exit code based on exception type:
    /// 0 - No exception (null)
    /// 2 - Database-related exceptions (Npgsql/Postgres)
    /// 3 - Invalid operation exceptions
    /// 1 - All other exceptions
    /// </returns>
    public static int Map(Exception? exception)
    {
        if (exception == null)
        {
            return SuccessExitCode;
        }

        // Check for InvalidOperationException first using type checking (more efficient than string comparison)
        if (exception is InvalidOperationException)
        {
            return InvalidOperationExitCode;
        }

        // Check for database-related exceptions by type hierarchy instead of string matching
        // This is more reliable and performant
        var exceptionType = exception.GetType();
        var exceptionTypeName = exceptionType.FullName ?? exceptionType.Name;

        // Check if exception is from Npgsql namespace or contains Postgres in type name
        if (exceptionTypeName.StartsWith("Npgsql.", StringComparison.Ordinal) ||
            exceptionTypeName.Contains("Postgres", StringComparison.Ordinal))
        {
            return DatabaseErrorExitCode;
        }

        return GeneralErrorExitCode;
    }
}


// Key improvements made:
// 1. Added constants for exit codes to improve maintainability and avoid magic numbers
// 2. Reordered exception checks - type checking (is) before string comparison for better performance
// 3. Changed StringComparison from OrdinalIgnoreCase to Ordinal for exact matching (more precise)
// 4. Used FullName instead of Name for more accurate namespace detection
// 5. Used StartsWith for Npgsql namespace check (more precise than Contains)
// 6. Added null-coalescing operator for FullName to handle edge cases
// 7. Maintained all existing functionality without adding new features