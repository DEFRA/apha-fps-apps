namespace AphaBatchJobs.Infrastructure.ErrorHandling;

/// <summary>
/// Constants class defining scheduler-friendly exit codes for different job execution outcomes.
/// These exit codes are used to communicate job execution status to external schedulers and monitoring systems.
/// </summary>
/// <remarks>
/// Exit codes follow standard Unix conventions where 0 indicates success and non-zero values indicate various failure conditions.
/// These codes enable schedulers to make intelligent decisions about job retry logic, alerting, and error handling.
/// </remarks>
public static class ExitCodeConstants
{
    /// <summary>
    /// Exit code for successful job execution.
    /// Indicates that the job completed all operations without errors.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Exit code for general job execution failure.
    /// Used when the job encounters an unexpected error that doesn't fall into specific error categories.
    /// </summary>
    public const int GeneralError = 1;

    /// <summary>
    /// Exit code for configuration validation failure.
    /// Indicates that required configuration settings are missing, invalid, or cannot be loaded.
    /// Jobs should not be retried automatically when this error occurs until configuration is corrected.
    /// </summary>
    public const int ConfigurationError = 2;

    /// <summary>
    /// Exit code for database connection or operation failure.
    /// Indicates issues with database connectivity, query execution, or transaction management.
    /// May be suitable for automatic retry depending on scheduler configuration.
    /// </summary>
    public const int DatabaseError = 3;

    /// <summary>
    /// Exit code for requested job not found error.
    /// Indicates that the specified job identifier does not match any registered job in the system.
    /// Jobs should not be retried automatically when this error occurs until job registration is verified.
    /// </summary>
    public const int JobNotFound = 4;

    /// <summary>
    /// Exit code for job execution timeout.
    /// Indicates that the job exceeded its maximum allowed execution time.
    /// Scheduler may retry with adjusted timeout settings or investigate performance issues.
    /// </summary>
    public const int TimeoutError = 5;
}


// Review Comments:
// 1. The code follows .NET naming conventions correctly (PascalCase for constants in a static class)
// 2. XML documentation is comprehensive and well-structured
// 3. Exit codes follow Unix conventions appropriately (0 = success, non-zero = failure)
// 4. The namespace follows standard .NET conventions
// 5. Constants are appropriately typed as int for exit codes
// 6. The class is properly marked as static since it only contains constants
// 7. Exit code values are sequential and logical, avoiding conflicts with standard system exit codes
// 8. No changes needed - the code is already following .NET best practices