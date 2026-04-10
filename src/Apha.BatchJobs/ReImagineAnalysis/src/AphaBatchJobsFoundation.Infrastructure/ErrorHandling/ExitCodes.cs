// ============================================================================
// File: ExitCodes.cs
// Description: Static class defining standard exit codes for scheduler 
//              integration following Unix conventions
// Project: AphaBatchJobsFoundation.Infrastructure
// ============================================================================

namespace AphaBatchJobsFoundation.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Defines standard exit codes for batch job execution following Unix conventions.
    /// These codes are used by schedulers and monitoring systems to determine job execution status.
    /// </summary>
    public static class ExitCodes
    {
        /// <summary>
        /// Indicates successful job execution with no errors.
        /// Exit code: 0
        /// </summary>
        public const int Success = 0;

        /// <summary>
        /// Indicates a general execution error that doesn't fall into specific categories.
        /// Exit code: 1
        /// </summary>
        public const int GeneralError = 1;

        /// <summary>
        /// Indicates configuration validation errors such as missing or invalid configuration values.
        /// Exit code: 2
        /// </summary>
        public const int ConfigurationError = 2;

        /// <summary>
        /// Indicates database connection or query execution errors.
        /// Exit code: 3
        /// </summary>
        public const int DatabaseError = 3;

        /// <summary>
        /// Indicates that the requested job was not found in the system.
        /// Exit code: 4
        /// </summary>
        public const int JobNotFound = 4;
    }
}


// Review Comments:
// ================
// The code is well-structured and follows .NET best practices. No changes are required because:
//
// 1. ✓ Naming Conventions: Follows PascalCase for class and constant names
// 2. ✓ Documentation: Comprehensive XML documentation comments for all members
// 3. ✓ Const Usage: Properly uses 'const' for immutable exit code values
// 4. ✓ Static Class: Appropriately declared as static since it contains only constants
// 5. ✓ Namespace: Follows standard .NET namespace conventions
// 6. ✓ Exit Code Values: Follows Unix conventions (0 = success, non-zero = errors)
// 7. ✓ Code Organization: Clean, readable, and maintainable structure
// 8. ✓ File Header: Includes appropriate metadata and description
//
// This is a simple constants class that doesn't interact with SQL Server directly,
// so SQL Server-specific best practices don't apply here. The code is production-ready.