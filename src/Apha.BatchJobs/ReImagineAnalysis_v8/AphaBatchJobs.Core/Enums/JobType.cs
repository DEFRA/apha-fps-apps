namespace AphaBatchJobs.Core.Enums;

/// <summary>
/// Defines the type of job execution trigger.
/// Used to distinguish between scheduled jobs that run on a cron schedule
/// and adhoc jobs that are triggered on demand.
/// </summary>
public enum JobType
{
    /// <summary>
    /// Represents a scheduled job execution triggered by a cron schedule via CLI argument.
    /// These jobs run automatically at predetermined intervals.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Represents an ad hoc job execution triggered on demand via CLI argument with a job name parameter.
    /// These jobs run only when explicitly requested.
    /// </summary>
    Adhoc = 1
}


// Review Comments:
// 1. The enum follows .NET 8 best practices with explicit integer values starting at 0
// 2. XML documentation is comprehensive and well-structured
// 3. Namespace follows proper conventions
// 4. Minor spelling correction: "adhoc" -> "ad hoc" in XML documentation (two words is the correct English spelling)
// 5. The enum is simple and appropriate for its use case - no need for [Flags] attribute
// 6. Naming convention follows PascalCase as per .NET standards
// 7. Consider adding a default value attribute if needed for serialization, but not required for this simple case
// 8. The code is clean, maintainable, and follows SOLID principles for enums