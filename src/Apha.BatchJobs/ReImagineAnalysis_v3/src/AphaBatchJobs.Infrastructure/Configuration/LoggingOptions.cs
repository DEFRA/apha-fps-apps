namespace AphaBatchJobs.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options class for structured logging settings.
    /// Provides configuration for log levels, output targets, and correlation id tracking.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Configuration section name for binding from appsettings.json
        /// </summary>
        public const string SectionName = "Logging";

        /// <summary>
        /// Gets or sets the minimum log level for the application.
        /// Valid values: Debug, Information, Warning, Error, Critical.
        /// Default: Information
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// Gets or sets a value indicating whether console logging is enabled.
        /// When true, logs will be written to the console output.
        /// Default: true
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether file-based logging is enabled.
        /// When true, logs will be written to file system.
        /// Default: false
        /// </summary>
        public bool EnableFileLogging { get; set; } = false;

        /// <summary>
        /// Gets or sets the header name used for correlation id tracking.
        /// This header is used to track requests across distributed systems.
        /// Default: X-Correlation-Id
        /// </summary>
        public string CorrelationIdHeader { get; set; } = "X-Correlation-Id";

        /// <summary>
        /// Validates the configuration options.
        /// </summary>
        /// <returns>True if configuration is valid; otherwise, false.</returns>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(MinimumLevel))
                return false;

            if (string.IsNullOrWhiteSpace(CorrelationIdHeader))
                return false;

            var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
            if (!validLogLevels.Contains(MinimumLevel, StringComparer.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}


**Key improvements made:**

1. **Added SectionName constant**: Following .NET configuration best practices, added a constant for the configuration section name to avoid magic strings when binding from appsettings.json.

2. **Added Validate() method**: Implemented validation logic to ensure configuration values are valid before use, following defensive programming practices common in enterprise .NET applications.

3. **Validation includes**:
   - Null/whitespace checks for required string properties
   - Log level validation against known valid values
   - Case-insensitive comparison for log levels

4. **Maintained existing functionality**: No new features added, only improvements to make the code more robust and idiomatic for .NET/AWS applications where configuration validation is critical for batch jobs.