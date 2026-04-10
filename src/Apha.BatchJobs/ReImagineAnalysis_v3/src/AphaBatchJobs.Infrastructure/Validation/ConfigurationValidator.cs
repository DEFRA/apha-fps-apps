using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AphaBatchJobs.Infrastructure.Validation
{
    /// <summary>
    /// Service to validate required configuration settings at startup including database, 
    /// logging, and job configuration to ensure application can run properly.
    /// Implements comprehensive validation with detailed error logging for troubleshooting.
    /// </summary>
    public class ConfigurationValidator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationValidator> _logger;

        // Configuration validation constants
        private const int MinCommandTimeout = 1;
        private const int MaxCommandTimeout = 3600;
        private const int MinRetryCount = 0;
        private const int MaxRetryCount = 10;
        private const int DefaultCommandTimeout = 30;
        private const int DefaultMaxRetryCount = 3;
        private const int DefaultTimeoutMinutes = 30;
        private const int DefaultMaxConcurrentJobs = 5;
        private const string DefaultMinimumLevel = "Information";
        private const string DefaultCorrelationIdHeader = "X-Correlation-Id";

        private static readonly string[] ValidLogLevels = { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };

        /// <summary>
        /// Constructor accepting IConfiguration and ILogger dependencies for configuration 
        /// access and logging.
        /// </summary>
        /// <param name="configuration">Application configuration instance</param>
        /// <param name="logger">Logger instance for validation error reporting</param>
        /// <exception cref="ArgumentNullException">Thrown when configuration or logger is null</exception>
        public ConfigurationValidator(
            IConfiguration configuration, 
            ILogger<ConfigurationValidator> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates all required configuration sections and returns true if all valid, 
        /// false otherwise with detailed error logging.
        /// Validates database, logging, and job configuration sections.
        /// </summary>
        /// <returns>True if all configuration is valid, false otherwise</returns>
        public Task<bool> ValidateAsync()
        {
            _logger.LogInformation("Starting configuration validation");

            try
            {
                var databaseValid = ValidateDatabaseConfiguration();
                var loggingValid = ValidateLoggingConfiguration();
                var jobValid = ValidateJobConfiguration();

                var isValid = databaseValid && loggingValid && jobValid;

                if (isValid)
                {
                    _logger.LogInformation("Configuration validation completed successfully");
                }
                else
                {
                    _logger.LogError("Configuration validation failed. Please check the errors above");
                }

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during configuration validation");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Validates database connection string is present and not empty, 
        /// logs errors if validation fails.
        /// </summary>
        /// <returns>True if database configuration is valid, false otherwise</returns>
        public bool ValidateDatabaseConfiguration()
        {
            _logger.LogDebug("Validating database configuration");

            var databaseSection = _configuration.GetSection(Configuration.DatabaseOptions.SectionName);
            
            if (!databaseSection.Exists())
            {
                _logger.LogError("Database configuration section '{SectionName}' is missing", 
                    Configuration.DatabaseOptions.SectionName);
                return false;
            }

            var connectionString = databaseSection.GetValue<string>("ConnectionString");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("Database connection string is missing or empty");
                return false;
            }

            var commandTimeout = databaseSection.GetValue<int>("CommandTimeout", DefaultCommandTimeout);
            if (commandTimeout < MinCommandTimeout || commandTimeout > MaxCommandTimeout)
            {
                _logger.LogError("Database CommandTimeout must be between {Min} and {Max} seconds. Current value: {CommandTimeout}", 
                    MinCommandTimeout, MaxCommandTimeout, commandTimeout);
                return false;
            }

            var maxRetryCount = databaseSection.GetValue<int>("MaxRetryCount", DefaultMaxRetryCount);
            if (maxRetryCount < MinRetryCount || maxRetryCount > MaxRetryCount)
            {
                _logger.LogError("Database MaxRetryCount must be between {Min} and {Max}. Current value: {MaxRetryCount}", 
                    MinRetryCount, MaxRetryCount, maxRetryCount);
                return false;
            }

            _logger.LogDebug("Database configuration is valid");
            return true;
        }

        /// <summary>
        /// Validates logging minimum level is valid enum value, 
        /// logs errors if validation fails.
        /// </summary>
        /// <returns>True if logging configuration is valid, false otherwise</returns>
        public bool ValidateLoggingConfiguration()
        {
            _logger.LogDebug("Validating logging configuration");

            var loggingSection = _configuration.GetSection(Configuration.LoggingOptions.SectionName);
            
            if (!loggingSection.Exists())
            {
                _logger.LogError("Logging configuration section '{SectionName}' is missing", 
                    Configuration.LoggingOptions.SectionName);
                return false;
            }

            var minimumLevel = loggingSection.GetValue<string>("MinimumLevel", DefaultMinimumLevel);
            
            if (string.IsNullOrWhiteSpace(minimumLevel))
            {
                _logger.LogError("Logging MinimumLevel is missing or empty");
                return false;
            }

            if (!ValidLogLevels.Any(level => string.Equals(level, minimumLevel, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogError("Logging MinimumLevel '{MinimumLevel}' is not a valid log level. Valid values are: {ValidLevels}", 
                    minimumLevel, string.Join(", ", ValidLogLevels));
                return false;
            }

            var correlationIdHeader = loggingSection.GetValue<string>("CorrelationIdHeader", DefaultCorrelationIdHeader);
            if (string.IsNullOrWhiteSpace(correlationIdHeader))
            {
                _logger.LogError("Logging CorrelationIdHeader is missing or empty");
                return false;
            }

            _logger.LogDebug("Logging configuration is valid");
            return true;
        }

        /// <summary>
        /// Validates job timeout and concurrency settings are positive integers, 
        /// logs errors if validation fails.
        /// </summary>
        /// <returns>True if job configuration is valid, false otherwise</returns>
        public bool ValidateJobConfiguration()
        {
            _logger.LogDebug("Validating job configuration");

            var jobSection = _configuration.GetSection(Configuration.JobOptions.SectionName);
            
            if (!jobSection.Exists())
            {
                _logger.LogError("Job configuration section '{SectionName}' is missing", 
                    Configuration.JobOptions.SectionName);
                return false;
            }

            var defaultTimeoutMinutes = jobSection.GetValue<int>("DefaultTimeoutMinutes", DefaultTimeoutMinutes);
            if (defaultTimeoutMinutes <= 0)
            {
                _logger.LogError("Job DefaultTimeoutMinutes must be a positive integer. Current value: {DefaultTimeoutMinutes}", 
                    defaultTimeoutMinutes);
                return false;
            }

            var maxConcurrentJobs = jobSection.GetValue<int>("MaxConcurrentJobs", DefaultMaxConcurrentJobs);
            if (maxConcurrentJobs <= 0)
            {
                _logger.LogError("Job MaxConcurrentJobs must be a positive integer. Current value: {MaxConcurrentJobs}", 
                    maxConcurrentJobs);
                return false;
            }

            _logger.LogDebug("Job configuration is valid");
            return true;
        }
    }
}


// Key improvements made:
// 1. Removed unnecessary 'await' in ValidateAsync() - no actual async operations are performed
// 2. Extracted magic numbers and strings to constants for better maintainability
// 3. Made ValidLogLevels static readonly to avoid recreating the array on each call
// 4. Replaced Array.Exists with LINQ Any() for more idiomatic .NET code
// 5. Used constants in log messages for consistency
// 6. Removed redundant variable assignment in ValidateAsync()
// 7. Added using System.Linq for LINQ operations
// 8. Improved code readability and maintainability without changing functionality