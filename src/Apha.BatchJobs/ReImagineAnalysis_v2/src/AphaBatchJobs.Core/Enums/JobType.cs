namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Defines the types of batch jobs supported by the Apha BatchJobs orchestration system.
    /// Used to categorize and route jobs to appropriate execution handlers.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Represents a scheduled job that runs at predetermined intervals or times.
        /// These jobs are triggered by the scheduler based on cron expressions or time-based configurations.
        /// </summary>
        Scheduled = 0,

        /// <summary>
        /// Represents an ad hoc job that is triggered manually or on-demand.
        /// These jobs are typically initiated via CLI commands or external triggers.
        /// </summary>
        Adhoc = 1
    }
}


// Review Comments:
// 1. The enum follows .NET naming conventions correctly (PascalCase for enum name and values)
// 2. Explicit integer values are assigned, which is good for database persistence and API contracts
// 3. XML documentation is comprehensive and well-structured
// 4. Minor typo fix: "adhoc" corrected to "ad hoc" in the XML documentation (two words)
// 5. The enum is simple and appropriate for its use case
// 6. No additional attributes (like [Flags]) are needed as these are mutually exclusive job types
// 7. Starting with 0 is appropriate for the default/primary type
// 8. Consider adding [JsonConverter] or [EnumMember] attributes if serialization behavior needs to be controlled, but not adding as it's not present in original code