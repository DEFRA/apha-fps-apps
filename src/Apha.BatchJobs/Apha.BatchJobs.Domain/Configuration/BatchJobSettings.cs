namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Batch job runtime settings.
/// </summary>
public sealed class BatchJobSettings
{
    /// <summary>
    /// Maximum number of concurrent jobs that can run simultaneously.
    /// Current runtime model is one ECS task per job execution, so this
    /// setting is intentionally out-of-scope for now.
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = 5;

    /// <summary>
    /// Job execution timeout in seconds.
    /// </summary>
    public int JobTimeout { get; set; } = 0;

    /// <summary>
    /// Optional per-job runtime timeout overrides in seconds.
    /// Keys are job names (for example: RecreateSummaries, MABArchive).
    /// </summary>
    public Dictionary<string, int> JobTimeoutOverridesSeconds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Number of retry attempts for failed jobs.
    /// Retry policy wiring is planned in a future story.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay in seconds between retry attempts.
    /// Retry policy wiring is planned in a future story.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Maximum total duration (in seconds) for all retry attempts combined.
    /// Prevents long-running containers from exceeding ECS task timeout.
    /// </summary>
    public int MaxRetryDurationSeconds { get; set; } = 0;

    /// <summary>
    /// Lock acquisition timeout in seconds for distributed locking.
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 0;

}
