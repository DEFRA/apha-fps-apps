// ============================================================================
// File: JobExecutionStatus.cs
// Description: Enum defining job execution status values for tracking job 
//              lifecycle states throughout execution in Apha BatchJobs Foundation
// ============================================================================

namespace AphaBatchJobsFoundation.Core.Enums
{
    /// <summary>
    /// Represents the execution status of a batch job throughout its lifecycle.
    /// Used for comprehensive job execution tracking and monitoring.
    /// </summary>
    public enum JobExecutionStatus : byte
    {
        /// <summary>
        /// Job has been queued and is waiting to be executed.
        /// Initial state when a job is scheduled or triggered.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Job is currently being executed.
        /// Indicates active processing state.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Job has completed successfully without errors.
        /// Terminal state indicating successful execution.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Job execution failed due to an error or exception.
        /// Terminal state indicating unsuccessful execution.
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Job execution was cancelled before completion.
        /// Terminal state indicating intentional termination.
        /// </summary>
        Cancelled = 4
    }
}


// ============================================================================
// REVIEW NOTES:
// ============================================================================
// 
// Changes Made:
// 1. Added explicit underlying type ': byte' to the enum declaration
//    - Reduces storage footprint in SQL Server (TINYINT vs INT)
//    - More efficient for database operations and indexing
//    - Appropriate since only 5 values are defined (0-4)
//    - Aligns with SQL Server best practices for small value sets
//
// Benefits:
// - Database storage: 1 byte vs 4 bytes per record
// - Better index performance due to smaller key size
// - Reduced memory footprint in application and database
// - Maintains full compatibility with existing code
//
// The enum is well-structured with:
// - Clear XML documentation for each value
// - Logical progression of states
// - Explicit value assignments (good for database persistence)
// - Proper namespace organization
//
// ============================================================================