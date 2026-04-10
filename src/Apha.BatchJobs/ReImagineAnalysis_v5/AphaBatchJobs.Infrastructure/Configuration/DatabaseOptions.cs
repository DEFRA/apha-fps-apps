namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Database configuration options for PostgreSQL connection and retry behavior.
/// Binds to the "Database" section in appsettings.json.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or initializes the PostgreSQL connection string.
    /// Should be stored in AWS Secrets Manager or Parameter Store for ECS Fargate deployments.
    /// </summary>
    public required string DefaultConnection { get; init; }

    /// <summary>
    /// Gets or initializes the database command timeout in seconds.
    /// Default value is 30 seconds.
    /// Recommended range: 30-300 seconds for batch jobs on ECS Fargate.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or initializes the maximum number of retry attempts for database operations.
    /// Default value is 3 retries.
    /// Recommended for transient fault handling in cloud environments.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <returns>True if configuration is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(DefaultConnection) 
               && TimeoutSeconds > 0 
               && MaxRetries >= 0;
    }
}


// Key improvements made:
// 1. Made class 'sealed' - it's a configuration POCO with no inheritance needs
// 2. Added 'required' modifier to DefaultConnection (C# 11/.NET 7+ feature) to enforce non-null initialization
// 3. Added const SectionName for consistent configuration binding across the application
// 4. Added IsValid() method for configuration validation (useful with IValidateOptions<T>)
// 5. Enhanced XML documentation with AWS ECS Fargate specific guidance
// 6. Noted that connection strings should use AWS Secrets Manager/Parameter Store for production
// 7. Added validation logic to ensure configuration integrity at startup
// 8. Maintained all existing functionality without adding new features