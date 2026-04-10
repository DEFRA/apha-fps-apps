using Microsoft.Extensions.Configuration;

namespace AphaBatchJobs.Infrastructure.Validation;

/// <summary>
/// Static utility class that validates configuration settings at startup.
/// Ensures that required configuration values are present and valid.
/// </summary>
public static class ConfigurationValidator
{
    /// <summary>
    /// Validates the configuration settings, specifically checking that the DefaultConnection string is present.
    /// </summary>
    /// <param name="configuration">The application configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the DefaultConnection string is null or empty.</exception>
    public static void Validate(IConfiguration configuration)
    {
        // Best Practice: Validate input parameters to prevent null reference exceptions
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        // Best Practice: Use GetConnectionString method which is the standard way to retrieve connection strings
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");

        // Best Practice: Use string.IsNullOrWhiteSpace instead of IsNullOrEmpty to catch whitespace-only strings
        // which are also invalid for connection strings
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            // Best Practice: Provide clear, actionable error messages for configuration issues
            // Include information about AWS Secrets Manager or Parameter Store for production environments
            throw new InvalidOperationException(
                "DefaultConnection string is not configured. " +
                "Please ensure ConnectionStrings:DefaultConnection is set in appsettings.json, " +
                "environment variables, AWS Secrets Manager, or AWS Systems Manager Parameter Store.");
        }

        // Best Practice: For PostgreSQL connections in AWS, optionally validate connection string format
        // This helps catch configuration errors early before attempting database connections
        ValidatePostgreSqlConnectionString(defaultConnection);
    }

    /// <summary>
    /// Validates that the connection string contains required PostgreSQL components.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection string format is invalid.</exception>
    private static void ValidatePostgreSqlConnectionString(string connectionString)
    {
        // Best Practice: Basic validation to ensure connection string has minimum required components
        // This prevents runtime errors when attempting to connect to PostgreSQL
        var lowerConnectionString = connectionString.ToLowerInvariant();
        
        if (!lowerConnectionString.Contains("host=") && !lowerConnectionString.Contains("server="))
        {
            throw new InvalidOperationException(
                "Invalid PostgreSQL connection string format. " +
                "Connection string must contain 'Host' or 'Server' parameter.");
        }

        if (!lowerConnectionString.Contains("database="))
        {
            throw new InvalidOperationException(
                "Invalid PostgreSQL connection string format. " +
                "Connection string must contain 'Database' parameter.");
        }
    }
}