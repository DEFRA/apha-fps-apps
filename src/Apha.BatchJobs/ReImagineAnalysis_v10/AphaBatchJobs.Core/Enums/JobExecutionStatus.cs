namespace AphaBatchJobs.Core.Enums;

/// <summary>
/// Defines the execution status of a batch job.
/// </summary>
public enum JobExecutionStatus
{
    /// <summary>
    /// Job completed successfully with no errors.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Job failed to complete due to errors.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Job completed with some errors or warnings but was not a complete failure.
    /// </summary>
    PartialSuccess = 2,

    /// <summary>
    /// Job was skipped and did not execute.
    /// </summary>
    Skipped = 3
}
