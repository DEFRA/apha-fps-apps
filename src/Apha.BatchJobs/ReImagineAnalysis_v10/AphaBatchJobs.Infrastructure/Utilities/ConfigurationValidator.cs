using Microsoft.Extensions.Configuration;

namespace AphaBatchJobs.Infrastructure.Utilities;

/// <summary>
/// Provides validation for application configuration settings.
/// </summary>
public static class ConfigurationValidator
{
    /// <summary>
    /// Validates that required configuration values are present and valid.
    /// </summary>
    /// <param name="configuration">The application configuration to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when configuration is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the DefaultConnection connection string is null or empty.
    /// </exception>
    public static void Validate(IConfiguration configuration)
    {
        // Best Practice: Validate method parameters to prevent null reference exceptions
        ArgumentNullException.ThrowIfNull(configuration);

        // Best Practice: Use nameof() for better refactoring support and type safety
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        
        // Best Practice: Use string.IsNullOrWhiteSpace instead of IsNullOrEmpty 
        // to also catch whitespace-only strings which are invalid for connection strings
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            // Best Practice: Include more specific error message for AWS/cloud deployments
            // mentioning common configuration sources (appsettings, environment variables, AWS Secrets Manager, Parameter Store)
            throw new InvalidOperationException(
                "DefaultConnection connection string is not configured. " +
                "Please ensure the ConnectionStrings:DefaultConnection setting is present in " +
                "appsettings.json, environment variables, AWS Secrets Manager, or AWS Systems Manager Parameter Store.");
        }

        // Best Practice: For PostgreSQL connection strings, consider validating the format
        // This ensures early detection of malformed connection strings before attempting database operations
        ValidatePostgreSqlConnectionString(defaultConnection);
    }

    /// <summary>
    /// Validates the basic structure of a PostgreSQL connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the connection string format is invalid.
    /// </exception>
    private static void ValidatePostgreSqlConnectionString(string connectionString)
    {
        // Best Practice: Basic validation to ensure connection string contains essential components
        // This prevents runtime errors when attempting to connect to PostgreSQL
        try
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            
            // Validate that at minimum, Host is specified
            if (string.IsNullOrWhiteSpace(builder.Host))
            {
                throw new InvalidOperationException(
                    "PostgreSQL connection string must contain a valid Host parameter.");
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Invalid PostgreSQL connection string format: {ex.Message}", ex);
        }
    }
}
