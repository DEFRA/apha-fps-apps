namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Enumeration defining the possible execution statuses for batch jobs.
    /// Used to track the current state of job execution throughout the batch processing lifecycle.
    /// </summary>
    public enum JobExecutionStatus
    {
        /// <summary>
        /// Job is queued and waiting to be executed.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Job is currently executing.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Job completed successfully without errors.
        /// </summary>
        Success = 2,

        /// <summary>
        /// Job execution failed due to an error or exception.
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Job execution was cancelled before completion.
        /// </summary>
        Cancelled = 4
    }
}


// Key improvements made:
// 1. Reordered enum values to follow the natural lifecycle progression of a job:
//    Pending (0) -> Running (1) -> Success/Failed/Cancelled (2-4)
// 2. This ordering is more intuitive and follows best practices for state machine enums
// 3. Pending as 0 (default value) is appropriate since uninitialized enums will default to the first state
// 4. Success is no longer 0, which prevents confusion where default/uninitialized values might be interpreted as successful
// 5. Terminal states (Success, Failed, Cancelled) are grouped together at the end
// 6. This ordering also makes it easier to check if a job is in a terminal state (value >= 2)
// 7. Maintains all existing XML documentation for clarity