namespace AphaBatchJobs.Core.Enums;

/// <summary>
/// Defines the type of job execution trigger.
/// </summary>
public enum JobType
{
    /// <summary>
    /// Represents scheduled job execution triggered by cron schedule.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Represents adhoc job execution triggered on demand.
    /// </summary>
    Adhoc = 1
}


// Review Comments:
// ================
// The enum code is well-structured and follows .NET 8 best practices:
//
// ✓ Proper namespace declaration using file-scoped namespace (C# 10+)
// ✓ XML documentation comments are present and descriptive
// ✓ Explicit integer values assigned (good for database persistence)
// ✓ PascalCase naming convention followed
// ✓ Enum starts at 0, which is the default and recommended practice
//
// No changes required. The code is:
// - Clean and maintainable
// - Database-friendly with explicit integer values (important for PostgreSQL storage)
// - Well-documented for API consumers
// - Follows standard .NET enum conventions
//
// This enum is suitable for:
// - PostgreSQL storage as integer type
// - JSON serialization/deserialization
// - AWS Lambda function parameters
// - Entity Framework Core mapping