namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the result of a job execution containing status information, 
/// message details, and an exit code for process termination.
/// </summary>
/// <param name="Status">The status of the job execution (e.g., "Success", "Failed", "Completed").</param>
/// <param name="Message">The execution message or error details. Can be null if no message is provided.</param>
/// <param name="ExitCode">The exit code of the job for process termination. Zero typically indicates success.</param>
public sealed record JobExecutionResult(string Status, string? Message, int ExitCode)
{
    /// <summary>
    /// Gets the status of the job execution.
    /// </summary>
    public string Status { get; init; } = Status ?? throw new ArgumentNullException(nameof(Status));

    /// <summary>
    /// Gets the execution message or error details.
    /// </summary>
    public string? Message { get; init; } = Message;

    /// <summary>
    /// Gets the exit code of the job for process termination.
    /// </summary>
    public int ExitCode { get; init; } = ExitCode;

    /// <summary>
    /// Creates a successful job execution result.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <returns>A JobExecutionResult indicating success.</returns>
    public static JobExecutionResult Success(string? message = null) 
        => new("Success", message, 0);

    /// <summary>
    /// Creates a failed job execution result.
    /// </summary>
    /// <param name="message">Error message describing the failure.</param>
    /// <param name="exitCode">Exit code indicating the type of failure. Defaults to 1.</param>
    /// <returns>A JobExecutionResult indicating failure.</returns>
    public static JobExecutionResult Failure(string message, int exitCode = 1) 
        => new("Failed", message ?? "An unknown error occurred", exitCode);
}


**Key improvements made:**

1. **Sealed record**: Added `sealed` keyword to prevent inheritance, which is a best practice for records that represent data transfer objects or value objects.

2. **Nullable annotation**: Changed `Message` parameter to `string?` to explicitly indicate it can be null, improving null-safety in .NET 8.

3. **Validation**: Added null check for `Status` parameter using init-only properties with validation to prevent null status values.

4. **Factory methods**: Added `Success()` and `Failure()` static factory methods for common use cases, making the API more intuitive and reducing code duplication.

5. **Enhanced XML documentation**: Improved parameter descriptions with more specific examples and clarifications.

6. **Explicit property declarations**: Made properties explicit with init accessors for better control and validation while maintaining record immutability.