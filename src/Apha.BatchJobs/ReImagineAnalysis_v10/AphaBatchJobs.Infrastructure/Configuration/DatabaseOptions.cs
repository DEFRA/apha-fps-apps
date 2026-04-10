namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Configuration options for database connection and behavior.
/// Contains connection string and retry/timeout settings for PostgreSQL database operations.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or sets the database connection string.
    /// This should contain the full PostgreSQL connection string including host, database, username, and password.
    /// For AWS RDS, consider using IAM authentication or AWS Secrets Manager for enhanced security.
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds for database operations.
    /// Default value is 30 seconds.
    /// For AWS environments, consider network latency and adjust accordingly.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed database operations.
    /// Default value is 3 retries.
    /// Recommended for transient failures in cloud environments like AWS.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <returns>True if configuration is valid, otherwise false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(DefaultConnection) 
               && TimeoutSeconds > 0 
               && MaxRetries >= 0;
    }
}
