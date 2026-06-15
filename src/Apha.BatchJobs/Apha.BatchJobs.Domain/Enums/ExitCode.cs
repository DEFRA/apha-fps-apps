namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Process exit codes for host and scheduler execution.
/// </summary>
public enum ExitCode
{
    /// <summary>
    /// Successful execution.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Non-specific runtime failure.
    /// </summary>
    GeneralError = 1,

    /// <summary>
    /// Invalid or missing configuration.
    /// </summary>
    ConfigurationError = 2,

    /// <summary>
    /// Database-related failure.
    /// </summary>
    DatabaseError = 3,

    /// <summary>
    /// Input or contract validation failure.
    /// </summary>
    ValidationError = 4,

    /// <summary>
    /// Unexpected unhandled exception.
    /// </summary>
    UnhandledException = 99
}
