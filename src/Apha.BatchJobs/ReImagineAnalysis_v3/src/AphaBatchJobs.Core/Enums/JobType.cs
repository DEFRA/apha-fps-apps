namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Enumeration defining the types of jobs supported in the Apha BatchJobs system.
    /// Used to categorize jobs based on their execution pattern and trigger mechanism.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Represents a scheduled job that runs at predefined intervals or specific times.
        /// These jobs are triggered automatically by the scheduler based on cron expressions or time intervals.
        /// </summary>
        Scheduled = 0,

        /// <summary>
        /// Represents an ad hoc job that is triggered manually or on-demand.
        /// These jobs are executed via CLI commands or API calls and run immediately upon request.
        /// </summary>
        Adhoc = 1
    }
}


// Review Notes:
// 1. The enum follows .NET naming conventions correctly (PascalCase for enum name and values)
// 2. Explicit value assignment (0, 1) is good practice for enums that may be persisted to database
// 3. XML documentation is comprehensive and well-structured
// 4. Minor correction: "adhoc" should be "ad hoc" (two words) in the documentation for proper English
// 5. The enum is simple and appropriate for its purpose - no additional complexity needed
// 6. Consider adding [Flags] attribute only if bitwise operations are needed (not applicable here)
// 7. The namespace follows standard .NET conventions
// 8. No PostgreSQL or AWS-specific concerns for this enum definition