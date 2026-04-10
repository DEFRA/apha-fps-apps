namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Configuration options for batch job execution settings.
/// Contains timeout and retry configuration for job operations.
/// </summary>
public sealed class JobOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "JobOptions";

    /// <summary>
    /// Gets or initializes the timeout duration in seconds for job execution.
    /// Default value is 300 seconds (5 minutes).
    /// </summary>
    public int TimeoutSeconds { get; init; } = 300;

    /// <summary>
    /// Gets or initializes the maximum number of retry attempts for failed job operations.
    /// Default value is 3 attempts.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <returns>True if configuration is valid, otherwise false.</returns>
    public bool IsValid()
    {
        return TimeoutSeconds > 0 && MaxRetries >= 0;
    }
}


**Key improvements made:**

1. **Sealed class**: Made the class `sealed` since it's a configuration POCO with no inheritance requirements, improving performance slightly
2. **Section name constant**: Added `SectionName` constant for consistent configuration binding (common .NET pattern)
3. **Default values**: Added sensible default values (300 seconds timeout, 3 retries) to prevent uninitialized configuration issues in ECS Fargate environments
4. **Validation method**: Added `IsValid()` method to support options validation pattern, which is important for fail-fast behavior in containerized environments
5. **Maintained existing structure**: Kept `init` accessors and all original properties as requested