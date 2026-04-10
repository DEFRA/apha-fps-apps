namespace AphaBatchJobs.Core.Enums;

/// <summary>
/// Defines the execution status of a batch job.
/// </summary>
public enum JobExecutionStatus
{
    /// <summary>
    /// Job executed successfully without any errors.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Job execution failed with errors.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Job executed with partial success, some operations succeeded while others failed.
    /// </summary>
    PartialSuccess = 2,

    /// <summary>
    /// Job execution was skipped.
    /// </summary>
    Skipped = 3
}


// Review Comments:
// ================
// The code is well-structured and follows .NET 8 best practices. No changes are required because:
//
// 1. ✅ Namespace follows proper naming conventions (PascalCase, hierarchical structure)
// 2. ✅ Enum uses PascalCase naming convention
// 3. ✅ XML documentation comments are comprehensive and properly formatted
// 4. ✅ Explicit integer values are assigned (important for database persistence with PostgreSQL)
// 5. ✅ Success = 0 follows convention (default value, falsy in boolean context)
// 6. ✅ File-scoped namespace is used (C# 10+ feature, supported in .NET 8)
// 7. ✅ Enum values are logically ordered and semantically clear
// 8. ✅ No [Flags] attribute (correctly, as these are mutually exclusive states)
//
// PostgreSQL Considerations:
// - Explicit integer values ensure consistent mapping to database integer columns
// - The enum can be stored as integer (recommended) or mapped to PostgreSQL enum type
// - Consider using Npgsql's enum mapping if storing as PostgreSQL native enum
//
// AWS/Distributed Systems Considerations:
// - Explicit values prevent issues when deploying across multiple services/regions
// - Clear status definitions support proper monitoring and alerting
//
// No refactoring needed - the code is production-ready.