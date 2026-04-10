using System.ComponentModel.DataAnnotations;

namespace AphaBatchJobs.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for PostgreSQL database connection settings.
    /// This class is bound to the "Database" section in appsettings.json.
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// Configuration section name for binding from appsettings.json
        /// </summary>
        public const string SectionName = "Database";

        /// <summary>
        /// PostgreSQL connection string.
        /// Required for database connectivity.
        /// </summary>
        [Required(ErrorMessage = "Database connection string is required")]
        [MinLength(1, ErrorMessage = "Database connection string cannot be empty")]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Database command timeout in seconds.
        /// Defines how long a command can execute before timing out.
        /// Default value is 30 seconds.
        /// </summary>
        [Range(1, 3600, ErrorMessage = "Command timeout must be between 1 and 3600 seconds")]
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Maximum retry count for database operations.
        /// Defines how many times a failed database operation should be retried.
        /// Default value is 3 retries.
        /// </summary>
        [Range(0, 10, ErrorMessage = "Max retry count must be between 0 and 10")]
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Enable or disable detailed database error logging.
        /// When enabled, provides verbose error information for troubleshooting.
        /// Should be disabled in production for security and performance.
        /// Default value is false.
        /// </summary>
        public bool EnableDetailedErrors { get; set; } = false;

        /// <summary>
        /// Validates the configuration options.
        /// </summary>
        /// <returns>True if configuration is valid, otherwise false.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ConnectionString) 
                   && CommandTimeout >= 1 && CommandTimeout <= 3600
                   && MaxRetryCount >= 0 && MaxRetryCount <= 10;
        }
    }
}


// Key improvements made:
// 1. Added MinLength validation to ConnectionString to prevent empty strings from passing Required validation
// 2. Added IsValid() method for programmatic validation beyond data annotations
// 3. Maintained all existing functionality and properties
// 4. Kept consistent with .NET configuration options pattern
// 5. All validation attributes remain compatible with IOptions validation in ASP.NET Core
// 6. No breaking changes to existing API surface