using CommandLine;

namespace AphaBatchJobsFoundationV3.Host.Configuration
{
    /// <summary>
    /// Model class defining command-line argument options for CLI trigger mode.
    /// Supports both scheduler and CLI execution modes for batch job orchestration.
    /// </summary>
    public sealed class CommandLineOptions
    {
        // Constants for valid mode values to avoid magic strings and improve maintainability
        /// <summary>
        /// Scheduler execution mode value.
        /// </summary>
        public const string SchedulerMode = "scheduler";

        /// <summary>
        /// Command-line immediate execution mode value.
        /// </summary>
        public const string CliMode = "cli";

        /// <summary>
        /// Gets or sets the execution mode for the batch job host.
        /// Valid values: "scheduler" (default) or "cli"
        /// - scheduler: Runs jobs based on configured schedules
        /// - cli: Executes a specific job immediately via command line
        /// </summary>
        [Option('m', "mode", Required = false, Default = SchedulerMode, HelpText = "Execution mode: scheduler or cli")]
        public string Mode { get; set; } = SchedulerMode;

        /// <summary>
        /// Gets or sets the name of the job to execute when running in CLI mode.
        /// This property is only used when Mode is set to "cli".
        /// The job name should match a registered job in the batch job system.
        /// </summary>
        [Option('j', "job", Required = false, HelpText = "Job name to execute in CLI mode")]
        public string? JobName { get; set; }
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to the class since it's not designed for inheritance
// 2. Introduced public constants for mode values ("scheduler" and "cli") to:
//    - Eliminate magic strings throughout the codebase
//    - Provide compile-time checking when referencing these values
//    - Improve maintainability and refactoring support
// 3. Updated the Default parameter in the Option attribute to use the constant
// 4. Updated the default property initializer to use the constant
// 5. Maintained nullable reference type annotation (string?) for JobName as it's optional
