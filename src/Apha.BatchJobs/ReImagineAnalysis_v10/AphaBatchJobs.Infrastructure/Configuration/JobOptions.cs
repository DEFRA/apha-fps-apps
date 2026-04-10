namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Configuration options for job execution behavior including timeout and retry settings.
/// </summary>
public sealed class JobOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json.
    /// </summary>
    public const string SectionName = "JobOptions";

    /// <summary>
    /// Gets or sets the timeout duration in seconds for job execution.
    /// Default value is 300 seconds (5 minutes).
    /// </summary>
    /// <remarks>
    /// Must be a positive value. Recommended range: 30-3600 seconds.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed jobs.
    /// Default value is 3 attempts.
    /// </summary>
    /// <remarks>
    /// Must be a non-negative value. Set to 0 to disable retries.
    /// </remarks>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <returns>True if configuration is valid; otherwise, false.</returns>
    public bool Validate()
    {
        return TimeoutSeconds > 0 && MaxRetries >= 0;
    }
}
