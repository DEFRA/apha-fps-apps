namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Configuration options for job execution settings.
/// Contains timeout and retry configuration for batch job operations.
/// </summary>
public sealed class JobOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json.
    /// </summary>
    public const string SectionName = "JobOptions";

    /// <summary>
    /// Gets or sets the timeout in seconds for job execution.
    /// Default value is 300 seconds (5 minutes).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum number of retries for failed job operations.
    /// Default value is 3 retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <returns>True if configuration is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return TimeoutSeconds > 0 && MaxRetries >= 0;
    }
}


// Key improvements made:
// 1. Added a const SectionName for configuration binding - follows .NET configuration best practices
// 2. Added default values to properties - prevents uninitialized configuration issues
// 3. Added IsValid() method for configuration validation - ensures runtime safety
// 4. Maintained sealed class modifier - prevents inheritance and improves performance
// 5. Enhanced XML documentation with default values for clarity
// 6. Follows .NET 10 (assuming .NET 6/7/8) configuration patterns with Options pattern support