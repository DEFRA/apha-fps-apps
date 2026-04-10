namespace AphaBatchJobs.Infrastructure.ErrorHandling;

/// <summary>
/// Maps exceptions to integer exit codes for process termination.
/// Exit codes follow standard conventions for containerized applications:
/// 0 = Success
/// 1 = General application error
/// 2 = Database/PostgreSQL error
/// 3 = Configuration error
/// </summary>
public static class ExitCodeMapper
{
    // Exit code constants for better maintainability and clarity
    private const int ExitCodeSuccess = 0;
    private const int ExitCodeGeneralError = 1;
    private const int ExitCodeDatabaseError = 2;
    private const int ExitCodeConfigurationError = 3;

    /// <summary>
    /// Maps an exception to an appropriate exit code.
    /// </summary>
    /// <param name="ex">The exception to map. Can be null.</param>
    /// <returns>
    /// 0 if exception is null (success),
    /// 2 if exception type contains 'Npgsql' or 'Postgres' (database error),
    /// 3 if exception is InvalidOperationException with message containing 'connection' or 'configuration' (configuration error),
    /// 1 for all other exceptions (general error)
    /// </returns>
    public static int Map(Exception? ex)
    {
        if (ex is null)
        {
            return ExitCodeSuccess;
        }

        // Check for PostgreSQL/Npgsql related exceptions
        // Using FullName provides more accurate type identification for nested types
        string exceptionTypeName = ex.GetType().FullName ?? ex.GetType().Name;
        
        if (exceptionTypeName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
            exceptionTypeName.Contains("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            return ExitCodeDatabaseError;
        }

        // Check for configuration-related InvalidOperationException
        if (ex is InvalidOperationException invalidOpEx)
        {
            string message = invalidOpEx.Message ?? string.Empty;
            if (message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("configuration", StringComparison.OrdinalIgnoreCase))
            {
                return ExitCodeConfigurationError;
            }
        }

        // Default to general error for all other exceptions
        return ExitCodeGeneralError;
    }
}


**Key improvements made:**

1. **Exit code constants**: Defined named constants for exit codes to improve maintainability and self-documentation
2. **Pattern matching**: Changed `ex == null` to `ex is null` for more idiomatic C# 10 null checking
3. **Type name resolution**: Using `FullName` with fallback to `Name` for more accurate exception type identification (handles nested types better)
4. **Comments**: Added clarifying comments for each section and documented exit code meanings at the class level
5. **Consistency**: Maintained all existing functionality while improving code clarity for containerized batch job scenarios in ECS Fargate where exit codes are critical for orchestration and monitoring