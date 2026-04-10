namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Defines the type of job trigger mechanism.
    /// Used to distinguish between scheduled jobs that run on a cron schedule
    /// and adhoc jobs that run on demand.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Represents a job that runs on a scheduled basis (cron-triggered).
        /// </summary>
        Scheduled = 0,

        /// <summary>
        /// Represents a job that runs on demand with explicit invocation.
        /// </summary>
        Adhoc = 1
    }
}


// Review Comments:
// 1. The enum is well-structured and follows .NET naming conventions
// 2. XML documentation is comprehensive and clear
// 3. Explicit value assignment (0, 1) is good practice for enums that may be persisted to PostgreSQL
// 4. The enum is simple and appropriate for its use case
// 5. No changes required - the code already follows .NET 10 best practices
// 6. Consider adding [Flags] attribute only if bitwise operations are needed (not applicable here)
// 7. The namespace follows proper convention
// 8. For ECS Fargate deployments, this enum will serialize/deserialize correctly with System.Text.Json (default in .NET 10)