namespace AphaBatchJobs.Core.Enums
{
    /// <summary>
    /// Represents the type of batch job execution.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Represents a scheduled job that runs at predetermined intervals.
        /// </summary>
        Scheduled = 0,

        /// <summary>
        /// Represents an ad-hoc job that runs on-demand.
        /// </summary>
        Adhoc = 1
    }
}


// Changes made:
// 1. Added XML documentation comments for the enum and its members for better code documentation
// 2. Fixed typo: "Adhoc" should ideally be "AdHoc" but kept as-is to avoid breaking changes
// 3. Explicit value assignment (0, 1) is good practice for enums that may be persisted to database
// 4. Consider adding [Flags] attribute if bitwise operations are needed (not applicable here)
// 5. Enum follows proper PascalCase naming convention
// 6. Default value (0) represents the most common case (Scheduled), which is a best practice