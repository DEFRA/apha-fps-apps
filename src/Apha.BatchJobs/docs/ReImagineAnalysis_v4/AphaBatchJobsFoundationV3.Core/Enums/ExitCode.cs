namespace AphaBatchJobsFoundationV3.Core.Enums
{
    /// <summary>
    /// Enumeration defining exit codes for batch job execution outcomes.
    /// Maps execution results to integer exit codes for process monitoring and orchestration.
    /// These exit codes are used by the host application to communicate execution status
    /// to external orchestration systems and monitoring tools.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// Indicates successful execution of the batch job.
        /// Exit code: 0
        /// </summary>
        Success = 0,

        /// <summary>
        /// Indicates a general error occurred during batch job execution.
        /// Exit code: 1
        /// </summary>
        GeneralError = 1,

        /// <summary>
        /// Indicates a configuration-related error occurred.
        /// This includes missing or invalid configuration settings.
        /// Exit code: 2
        /// </summary>
        ConfigurationError = 2,

        /// <summary>
        /// Indicates a database-related error occurred.
        /// This includes connection failures, query errors, or data access issues.
        /// Exit code: 3
        /// </summary>
        DatabaseError = 3,

        /// <summary>
        /// Indicates a validation error occurred.
        /// This includes input validation failures or business rule violations.
        /// Exit code: 4
        /// </summary>
        ValidationError = 4,

        /// <summary>
        /// Indicates an unhandled exception occurred during execution.
        /// This represents unexpected errors that were not caught by specific error handlers.
        /// Exit code: 99
        /// </summary>
        UnhandledException = 99
    }
}


// Review Comments:
// The code is already well-written and follows .NET best practices:
// 1. ✓ Proper namespace organization following project structure
// 2. ✓ PascalCase naming convention for enum and its members
// 3. ✓ Comprehensive XML documentation for the enum and all members
// 4. ✓ Explicit integer values assigned to enum members (good for stability)
// 5. ✓ Logical exit code values (0 for success, positive integers for errors)
// 6. ✓ Clear and descriptive member names
// 7. ✓ No trailing commas after the last enum member (correct syntax)
// 8. ✓ Appropriate use of enum for representing a fixed set of exit codes
//
// No changes are required. The code adheres to .NET conventions and best practices.