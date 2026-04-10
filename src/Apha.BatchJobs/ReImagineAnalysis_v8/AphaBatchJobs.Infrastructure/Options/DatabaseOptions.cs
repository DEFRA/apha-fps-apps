namespace AphaBatchJobs.Infrastructure.Options;

/// <summary>
/// Sealed configuration class for database connection settings.
/// Contains connection string and retry configuration for PostgreSQL database operations.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or sets the PostgreSQL database connection string.
    /// This should contain the full connection string including host, database, username, and password.
    /// </summary>
    /// <remarks>
    /// For AWS deployments, consider using AWS Secrets Manager or Parameter Store for sensitive connection strings.
    /// Connection string should follow Npgsql format: "Host=myserver;Database=mydb;Username=myuser;Password=mypass"
    /// </remarks>
    public string DefaultConnection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds for database operations.
    /// Default value is 30 seconds.
    /// </summary>
    /// <remarks>
    /// Recommended range: 30-300 seconds depending on query complexity and network latency in AWS environments.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed database operations.
    /// Default value is 3 retries.
    /// </summary>
    /// <remarks>
    /// Recommended for transient failures in AWS RDS PostgreSQL environments.
    /// Consider exponential backoff strategy when implementing retry logic.
    /// </remarks>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the minimum pool size for database connections.
    /// Default value is 0 (no minimum).
    /// </summary>
    /// <remarks>
    /// For AWS Lambda or batch jobs, keep this at 0 to avoid maintaining idle connections.
    /// For long-running services, consider setting to 1-5 for better performance.
    /// </remarks>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum pool size for database connections.
    /// Default value is 100.
    /// </summary>
    /// <remarks>
    /// For AWS RDS PostgreSQL, ensure this doesn't exceed max_connections setting.
    /// Consider AWS RDS instance size and concurrent connection limits.
    /// </remarks>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Validates the database options configuration.
    /// </summary>
    /// <returns>True if configuration is valid, otherwise false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(DefaultConnection) 
               && TimeoutSeconds > 0 
               && MaxRetries >= 0
               && MinPoolSize >= 0
               && MaxPoolSize > 0
               && MaxPoolSize >= MinPoolSize;
    }
}