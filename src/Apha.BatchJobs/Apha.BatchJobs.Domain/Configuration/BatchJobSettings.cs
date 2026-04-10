namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Batch job runtime settings.
/// </summary>
public sealed class BatchJobSettings
{
    /// <summary>
    /// Maximum number of concurrent jobs that can run simultaneously.
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = 5;

    /// <summary>
    /// Job execution timeout in seconds.
    /// </summary>
    public int JobTimeout { get; set; } = 3600;

    /// <summary>
    /// Number of retry attempts for failed jobs.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay in seconds between retry attempts.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Lock acquisition timeout in seconds for distributed locking.
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 300;
}
