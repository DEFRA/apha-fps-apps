using Microsoft.Extensions.Configuration;

namespace AphaBatchJobs.Infrastructure.Validation;

/// <summary>
/// Static utility class for validating configuration settings at startup.
/// </summary>
public static class ConfigurationValidator
{
    /// <summary>
    /// Validates that required configuration settings are present and valid.
    /// </summary>
    /// <param name="configuration">The application configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when DefaultConnection is null or empty.</exception>
    public static void Validate(IConfiguration configuration)
    {
        // Best Practice: Validate input parameters to prevent null reference exceptions
        ArgumentNullException.ThrowIfNull(configuration);

        var defaultConnection = configuration.GetConnectionString("DefaultConnection");

        // Best Practice: Use IsNullOrWhiteSpace instead of IsNullOrEmpty to catch whitespace-only strings
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException(
                "DefaultConnection string is not configured. Please ensure ConnectionStrings:DefaultConnection is set in appsettings.json or environment variables.");
        }

        // Best Practice: Validate PostgreSQL connection string format for AWS RDS
        // This ensures the connection string contains essential components
        ValidatePostgreSqlConnectionString(defaultConnection);
    }

    /// <summary>
    /// Validates that the PostgreSQL connection string contains required components.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when connection string is invalid.</exception>
    private static void ValidatePostgreSqlConnectionString(string connectionString)
    {
        // Best Practice: Basic validation to ensure connection string has key components
        // This helps catch configuration errors early in AWS environments
        var lowerConnectionString = connectionString.ToLowerInvariant();
        
        if (!lowerConnectionString.Contains("host=") && !lowerConnectionString.Contains("server="))
        {
            throw new InvalidOperationException(
                "DefaultConnection string must contain a 'Host' or 'Server' parameter for PostgreSQL connection.");
        }

        if (!lowerConnectionString.Contains("database="))
        {
            throw new InvalidOperationException(
                "DefaultConnection string must contain a 'Database' parameter for PostgreSQL connection.");
        }
    }
}