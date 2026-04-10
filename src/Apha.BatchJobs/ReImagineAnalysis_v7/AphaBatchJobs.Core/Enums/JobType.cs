namespace AphaBatchJobs.Core.Enums;

/// <summary>
/// Defines the type of job execution trigger.
/// Used to categorize how a batch job was initiated.
/// </summary>
public enum JobType
{
    /// <summary>
    /// Job triggered by a scheduled cron expression.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Job triggered on-demand with explicit job name parameter.
    /// </summary>
    Adhoc = 1
}


// Review Comments:
// ================
// The enum code is well-structured and follows .NET best practices:
//
// 1. ✓ Proper namespace declaration using file-scoped namespace (modern C# style)
// 2. ✓ XML documentation comments are present and descriptive
// 3. ✓ Explicit integer values assigned (0, 1) - good for database persistence
// 4. ✓ PascalCase naming convention followed for enum and members
// 5. ✓ Meaningful enum member names (Scheduled, Adhoc)
// 6. ✓ Default value (Scheduled = 0) is semantically appropriate
//
// Note: The typo "AphaBatchJobs" (should likely be "AlphaBatchJobs") is in the 
// namespace, but as per instructions, we're not changing existing functionality 
// or structure - only reviewing for best practices compliance.
//
// No changes required - the code already follows .NET and enum best practices.