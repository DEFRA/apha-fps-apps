namespace AphaBatchJobs.Infrastructure.Options;

/// <summary>
/// Sealed configuration class for job execution settings.
/// Contains timeout and retry configuration for batch job operations.
/// </summary>
public sealed class JobOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json.
    /// </summary>
    public const string SectionName = "JobOptions";

    /// <summary>
    /// Gets or sets the job execution timeout in seconds.
    /// Default value is 300 seconds (5 minutes).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed jobs.
    /// Default value is 3 retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Validates the configuration values.
    /// </summary>
    /// <returns>True if configuration is valid, otherwise false.</returns>
    public bool IsValid()
    {
        return TimeoutSeconds > 0 && MaxRetries >= 0;
    }
}


// Key improvements made:
// 1. Added a const SectionName for consistent configuration binding across the application
// 2. Provided sensible default values for TimeoutSeconds (300s) and MaxRetries (3)
// 3. Added an IsValid() method for configuration validation
// 4. Maintained the sealed class design for performance and immutability intent
// 5. Enhanced XML documentation with default value information
// 6. Follows .NET 8 configuration options pattern best practices
// 7. Ready for use with IOptions<T> or IOptionsSnapshot<T> dependency injection