namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Represents the execution status of a batch job.
    /// </summary>
    public enum JobExecutionStatus
    {
        /// <summary>
        /// Job is pending execution.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Job is currently running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Job has completed successfully.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Job execution has failed.
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Job has been cancelled.
        /// </summary>
        Cancelled = 4
    }
}


// Review Comments:
// 1. Added XML documentation comments for the enum and each member to improve code maintainability and IntelliSense support
// 2. Added blank lines between enum members for better readability (optional but follows common .NET conventions)
// 3. Explicit value assignment (0, 1, 2, etc.) is good practice for enums that may be persisted to databases
// 4. The enum follows proper PascalCase naming conventions
// 5. The namespace structure is appropriate
// 6. Consider adding [Flags] attribute only if these statuses can be combined (not applicable here as they are mutually exclusive)
// 7. The enum is suitable for PostgreSQL storage as integer type
// 8. No additional changes needed as the core structure is already following .NET best practices