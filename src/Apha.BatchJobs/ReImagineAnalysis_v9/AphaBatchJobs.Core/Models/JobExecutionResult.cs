namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the result of a job execution containing status, message, and exit code information.
/// </summary>
/// <param name="Status">The status of the job execution</param>
/// <param name="Message">The execution message or error details</param>
/// <param name="ExitCode">The exit code of the job execution</param>
public sealed record JobExecutionResult(string Status, string Message, int ExitCode)
{
    /// <summary>
    /// Gets the status of the job execution.
    /// </summary>
    public string Status { get; init; } = Status ?? throw new ArgumentNullException(nameof(Status));

    /// <summary>
    /// Gets the execution message or error details.
    /// </summary>
    public string Message { get; init; } = Message ?? throw new ArgumentNullException(nameof(Message));

    /// <summary>
    /// Gets the exit code of the job execution.
    /// </summary>
    public int ExitCode { get; init; } = ExitCode;
}


**Key improvements made:**

1. **Sealed modifier**: Added `sealed` keyword to prevent inheritance, which is a best practice for records that represent data transfer objects and improves performance by allowing the compiler to make optimizations.

2. **Null validation**: Added null checks for string properties in the primary constructor to prevent null reference exceptions. This follows defensive programming practices and ensures data integrity.

3. **Explicit property declarations**: Made properties explicit with XML documentation and `init` accessors for better clarity and IntelliSense support, while maintaining immutability.

4. **Validation at construction**: By validating in the property initializers, we ensure that invalid `JobExecutionResult` instances cannot be created, following the fail-fast principle.

These changes maintain the existing functionality while making the code more robust and aligned with .NET 8 best practices for immutable data models.