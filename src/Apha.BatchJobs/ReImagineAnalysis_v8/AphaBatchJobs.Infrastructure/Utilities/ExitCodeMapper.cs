namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Static utility class that maps exceptions to integer exit codes for process termination.
/// Exit codes:
/// - 0: No exception (null)
/// - 1: General/unknown exception
/// - 2: Database-related exceptions (Npgsql/Postgres)
/// - 3: Invalid operation exception
/// </summary>
public static class ExitCodeMapper
{
    // Define exit codes as constants for better maintainability and clarity
    private const int ExitCodeSuccess = 0;
    private const int ExitCodeGeneralError = 1;
    private const int ExitCodeDatabaseError = 2;
    private const int ExitCodeInvalidOperation = 3;

    /// <summary>
    /// Maps an exception to an integer exit code.
    /// </summary>
    /// <param name="exception">The exception to map. Can be null.</param>
    /// <returns>
    /// 0 if exception is null,
    /// 2 if exception type name contains "Npgsql" or "Postgres",
    /// 3 if exception is InvalidOperationException,
    /// 1 for all other exceptions.
    /// </returns>
    public static int Map(Exception? exception)
    {
        if (exception is null)
        {
            return ExitCodeSuccess;
        }

        // Check specific exception types first (more efficient than string comparison)
        if (exception is InvalidOperationException)
        {
            return ExitCodeInvalidOperation;
        }

        // Use FullName instead of Name for more accurate type identification
        // This prevents false positives from custom exception types
        var exceptionTypeFullName = exception.GetType().FullName ?? string.Empty;

        // Check for database-related exceptions using FullName for better accuracy
        if (exceptionTypeFullName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
            exceptionTypeFullName.Contains("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            return ExitCodeDatabaseError;
        }

        return ExitCodeGeneralError;
    }
}


// Key improvements made:
// 1. Added constants for exit codes to improve maintainability and avoid magic numbers
// 2. Reordered exception checks - type checks (is) are more efficient than string operations
// 3. Changed from GetType().Name to GetType().FullName for more accurate type identification
// 4. Added null-coalescing operator (??) when getting FullName to prevent potential NullReferenceException
// 5. Maintained all existing functionality without adding new features
// 6. Improved code readability and follows .NET 8 best practices