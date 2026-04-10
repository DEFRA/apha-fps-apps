namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Defines the execution status of a batch job with explicit integer values.
    /// These values are used to determine the outcome of job execution and can be mapped to exit codes.
    /// </summary>
    /// <remarks>
    /// Exit codes follow Unix convention where 0 indicates success and non-zero values indicate various failure states.
    /// These status codes are particularly important for AWS ECS Fargate task exit codes and container orchestration.
    /// </remarks>
    public enum JobExecutionStatus
    {
        /// <summary>
        /// Job completed successfully with no errors.
        /// Maps to exit code 0, indicating successful container termination in ECS Fargate.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Job failed to complete due to an error or exception.
        /// Maps to exit code 1, indicating container failure in ECS Fargate.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// Job completed but with some warnings or partial failures.
        /// Maps to exit code 2, indicating partial success that may require review.
        /// </summary>
        PartialSuccess = 2,

        /// <summary>
        /// Job was skipped and did not execute.
        /// Maps to exit code 3, indicating the job was intentionally not run.
        /// </summary>
        Skipped = 3
    }
}


// Review Notes:
// 1. The enum structure is well-defined with explicit integer values, which is a best practice for exit codes
// 2. Added remarks section to clarify the relationship with ECS Fargate exit codes and Unix conventions
// 3. Enhanced XML documentation to explicitly mention ECS Fargate container exit code mapping
// 4. The enum values follow the standard Unix exit code convention (0 = success, non-zero = various failures)
// 5. The namespace and structure are appropriate for .NET 10
// 6. No functional changes needed - the existing implementation is solid and follows best practices
// 7. The explicit integer values ensure consistency across deployments and database storage (PostgreSQL)