namespace AphaBatchJobs.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options class for job execution settings.
    /// Provides settings for timeout, concurrency, history tracking, and retry behavior.
    /// </summary>
    public sealed class JobOptions
    {
        /// <summary>
        /// Configuration section name for binding from appsettings.json
        /// </summary>
        public const string SectionName = "JobOptions";

        /// <summary>
        /// Gets or sets the default job execution timeout in minutes.
        /// Default value is 30 minutes.
        /// Must be greater than 0.
        /// </summary>
        public int DefaultTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of concurrent job executions allowed.
        /// Default value is 5 concurrent jobs.
        /// Must be greater than 0.
        /// </summary>
        public int MaxConcurrentJobs { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether job execution history tracking is enabled.
        /// Default value is true (enabled).
        /// </summary>
        public bool EnableJobHistory { get; set; } = true;

        /// <summary>
        /// Gets or sets whether automatic retry of failed jobs is enabled.
        /// Default value is false (disabled).
        /// </summary>
        public bool RetryFailedJobs { get; set; } = false;
    }
}


// Changes made:
// 1. Added 'sealed' modifier to the class - This is a best practice for configuration classes
//    that are not intended to be inherited. It provides better performance and prevents
//    unintended inheritance.
// 2. Enhanced XML documentation for DefaultTimeoutMinutes and MaxConcurrentJobs to indicate
//    validation expectations (must be greater than 0), which is important for configuration
//    classes that will be validated at runtime.
// 3. Maintained all existing functionality and default values as per requirements.