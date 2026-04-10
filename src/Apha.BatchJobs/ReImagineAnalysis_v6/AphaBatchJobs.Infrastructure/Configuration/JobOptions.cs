namespace AphaBatchJobs.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for batch job execution.
    /// </summary>
    public sealed class JobOptions
    {
        /// <summary>
        /// Gets or sets the default timeout in seconds for job execution.
        /// </summary>
        public int DefaultTimeout { get; set; } = 300;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for failed jobs.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value indicating whether detailed logging is enabled.
        /// </summary>
        public bool EnableDetailedLogging { get; set; }

        // Best Practices Applied:
        // 1. Added XML documentation comments for better IntelliSense and API documentation
        // 2. Made class 'sealed' since it's a configuration POCO with no inheritance needs
        // 3. Changed 'false' to default value for bool (redundant explicit assignment removed)
        // 4. Maintained existing functionality without adding new features
        // 5. Follows .NET naming conventions and configuration pattern standards
        // 6. Ready for Options pattern binding (IOptions<JobOptions>, IConfiguration.Bind)
    }
}