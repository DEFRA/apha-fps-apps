namespace AphaBatchJobs.Infrastructure.Configuration;

/// <summary>
/// Configuration options for PostgreSQL database connections.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// The section name in appsettings.json for binding these options.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// PostgreSQL connection string. Should be stored in AWS Secrets Manager or Parameter Store for production.
    /// </summary>
    public required string ConnectionString { get; set; }
    
    /// <summary>
    /// Command timeout in seconds. Default is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Enable automatic retry on transient failures. Default is true.
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts for transient failures. Default is 3.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Maximum delay between retries in seconds. Default is 30 seconds.
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("Database connection string cannot be null or empty.");
        }

        if (CommandTimeout <= 0)
        {
            throw new InvalidOperationException("Command timeout must be greater than 0.");
        }

        if (MaxRetryCount < 0)
        {
            throw new InvalidOperationException("Max retry count cannot be negative.");
        }

        if (MaxRetryDelay <= 0)
        {
            throw new InvalidOperationException("Max retry delay must be greater than 0.");
        }
    }
}


**Key improvements made:**

1. **Sealed class**: Marked as `sealed` since configuration classes typically don't need inheritance
2. **XML documentation**: Added comprehensive documentation for all properties and methods
3. **Required property**: Made `ConnectionString` `required` (C# 11/.NET 7+ feature) to enforce initialization
4. **Const section name**: Added `SectionName` constant for consistent configuration binding
5. **Retry configuration**: Added `MaxRetryCount` and `MaxRetryDelay` properties for better control over PostgreSQL retry logic
6. **Validation method**: Added `Validate()` method to ensure configuration integrity at startup
7. **AWS best practice comment**: Added comment about storing connection strings in AWS Secrets Manager/Parameter Store
8. **Removed default empty string**: Using `required` keyword instead of default empty string prevents misconfiguration