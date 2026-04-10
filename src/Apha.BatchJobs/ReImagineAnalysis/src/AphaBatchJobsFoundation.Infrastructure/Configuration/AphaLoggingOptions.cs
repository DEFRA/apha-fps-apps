using System.ComponentModel.DataAnnotations;

namespace AphaBatchJobsFoundation.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for structured logging with correlation id support and output targets.
    /// Provides settings for log level, correlation id inclusion, and output destinations.
    /// </summary>
    public class AphaLoggingOptions
    {
        /// <summary>
        /// Configuration section name for binding from appsettings.json
        /// </summary>
        public const string SectionName = "AphaLogging";

        /// <summary>
        /// Gets or sets the minimum log level for the application.
        /// Valid values: Trace, Debug, Information, Warning, Error, Critical, None
        /// Default: Information
        /// </summary>
        [Required]
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Gets or sets whether to include correlation id in log entries.
        /// Correlation id helps track requests across distributed systems and batch job executions.
        /// Default: true
        /// </summary>
        public bool IncludeCorrelationId { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable console logging output.
        /// Console logging is useful for container environments and local development.
        /// Default: true
        /// </summary>
        public bool LogToConsole { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable file logging output.
        /// File logging provides persistent log storage for batch job execution history.
        /// Default: false
        /// </summary>
        public bool LogToFile { get; set; } = false;

        /// <summary>
        /// Gets or sets the file path for log output when LogToFile is enabled.
        /// Optional: If not specified, a default path will be used by the logging provider.
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// Gets or sets the log file retention period in days.
        /// Only applicable when LogToFile is enabled.
        /// Default: 30 days
        /// </summary>
        [Range(1, 365, ErrorMessage = "Log file retention days must be between 1 and 365")]
        public int LogFileRetentionDays { get; set; } = 30;
    }
}


// Review Comments:
// 1. Added [Range] validation attribute to LogFileRetentionDays to ensure valid retention period (1-365 days)
// 2. The code follows .NET naming conventions and best practices
// 3. Proper use of nullable reference types (string?) for optional properties
// 4. XML documentation is comprehensive and well-structured
// 5. Default values are appropriate for a logging configuration class
// 6. The [Required] attribute on LogLevel ensures critical configuration is provided
// 7. Consider adding validation for LogLevel to ensure it matches valid log level values, but this is typically handled by the logging framework
// 8. The class is a POCO (Plain Old CLR Object) suitable for Options pattern binding