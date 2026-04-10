// ============================================================================
// File: JobType.cs
// Description: Enum defining job types to distinguish between scheduled and 
//              adhoc execution patterns in Apha BatchJobs Foundation
// ============================================================================

namespace AphaBatchJobsFoundation.Core.Enums
{
    /// <summary>
    /// Defines the execution type for batch jobs in the Apha BatchJobs system.
    /// Used to distinguish between scheduled jobs that run on a timer and 
    /// adhoc jobs that are triggered manually or on-demand.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Represents a job that runs on a predefined schedule (e.g., cron expression, timer).
        /// Scheduled jobs are typically configured to execute at specific intervals or times.
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Represents a job that is triggered manually or on-demand.
        /// Adhoc jobs are executed outside of any regular schedule, typically via CLI or API trigger.
        /// </summary>
        Adhoc = 2
    }
}


// Review Comments:
// ================
// The code is already well-structured and follows .NET best practices:
//
// 1. ✓ Proper namespace organization
// 2. ✓ Clear XML documentation comments for IntelliSense support
// 3. ✓ Explicit integer values assigned to enum members (good for database persistence)
// 4. ✓ PascalCase naming convention for enum and members
// 5. ✓ Meaningful and descriptive names
// 6. ✓ Starting enum values at 1 (avoiding 0 for explicit intent, useful for SQL Server)
//
// No changes are required. The enum is production-ready and follows both .NET and
// SQL Server integration best practices, particularly:
// - Explicit integer values ensure consistent database mapping
// - Non-zero starting value helps distinguish between "not set" (0/null) and valid values
// - Simple integer type is optimal for SQL Server storage and indexing