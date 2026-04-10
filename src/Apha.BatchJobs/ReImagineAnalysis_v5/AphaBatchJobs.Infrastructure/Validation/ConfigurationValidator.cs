using Microsoft.Extensions.Configuration;
using System;

namespace AphaBatchJobs.Infrastructure.Validation
{
    /// <summary>
    /// Static validator class for configuration validation.
    /// Ensures required configuration values are present before application execution.
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Validates that the DefaultConnection connection string is configured.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when configuration parameter is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when DefaultConnection connection string is null, empty, or whitespace.
        /// </exception>
        public static void Validate(IConfiguration configuration)
        {
            // Best Practice: Validate method parameters to prevent null reference exceptions
            ArgumentNullException.ThrowIfNull(configuration);
            
            // Best Practice: Use GetConnectionString for connection strings instead of GetSection
            var defaultConnection = configuration.GetConnectionString("DefaultConnection");
            
            // Best Practice: Validate critical configuration early in application startup
            // This follows the fail-fast principle for ECS Fargate deployments
            if (string.IsNullOrWhiteSpace(defaultConnection))
            {
                // Best Practice: Provide clear error messages for configuration issues
                // This helps with troubleshooting in AWS CloudWatch logs
                throw new InvalidOperationException(
                    "DefaultConnection connection string is not configured. " +
                    "Ensure the connection string is set via environment variables, " +
                    "AWS Secrets Manager, or appsettings.json for ECS Fargate deployment.");
            }
            
            // Best Practice: For PostgreSQL on ECS Fargate, validate connection string format
            // This helps catch configuration errors before attempting database connections
            if (!defaultConnection.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "DefaultConnection connection string appears to be invalid. " +
                    "PostgreSQL connection strings must contain 'Host=' parameter.");
            }
        }
    }
}