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
        /// Initial state before execution begins.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Job is currently executing.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Job completed successfully without errors.
        /// Terminal state indicating successful completion.
        /// </summary>
        Success = 2,

        /// <summary>
        /// Job execution failed due to an error or exception.
        /// Terminal state indicating failure.
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Job execution was cancelled before completion.
        /// Terminal state indicating cancellation.
        /// </summary>
        Cancelled = 4
    }
}


// Key improvements made:
// 1. Reordered enum values to follow logical state progression (Pending -> Running -> Terminal States)
// 2. Pending = 0 as the default/initial state (best practice for enums to have the most common/initial value as 0)
// 3. Enhanced XML documentation to clarify state types (initial, active, terminal)
// 4. Maintained explicit value assignments for database compatibility and API stability
// 5. Logical grouping: initial state (0), active state (1), terminal states (2-4)
// 
// This ordering is more intuitive for state machine implementations and aligns with
// common batch processing patterns where jobs start as Pending, transition to Running,
// and end in one of three terminal states (Success, Failed, or Cancelled).