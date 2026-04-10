namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Configuration class for database connection settings.
/// Contains connection string and retry policy settings for PostgreSQL database operations.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or sets the default database connection string.
    /// This should contain the PostgreSQL connection string with host, database, username, and password.
    /// </summary>
    /// <remarks>
    /// For AWS deployments, consider using AWS Secrets Manager or Parameter Store for sensitive connection strings.
    /// Connection string format: Host=myserver;Database=mydb;Username=myuser;Password=mypass;
    /// </remarks>
    public required string DefaultConnection { get; set; }

    /// <summary>
    /// Gets or sets the timeout in seconds for database operations.
    /// Default value is 30 seconds.
    /// </summary>
    /// <remarks>
    /// For AWS RDS PostgreSQL, consider network latency and adjust timeout accordingly.
    /// Recommended range: 30-60 seconds for batch operations.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed database operations.
    /// Default value is 3 retries.
    /// </summary>
    /// <remarks>
    /// Implements exponential backoff retry pattern for transient failures.
    /// Suitable for handling temporary AWS RDS connection issues.
    /// </remarks>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the minimum pool size for connection pooling.
    /// Default value is 0 (no minimum connections maintained).
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum pool size for connection pooling.
    /// Default value is 100 connections.
    /// </summary>
    /// <remarks>
    /// For AWS RDS, ensure this value doesn't exceed the max_connections parameter.
    /// Monitor RDS connection metrics and adjust accordingly.
    /// </remarks>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether SSL mode is enabled for the connection.
    /// Default value is true for secure AWS RDS connections.
    /// </summary>
    public bool EnableSsl { get; set; } = true;
}