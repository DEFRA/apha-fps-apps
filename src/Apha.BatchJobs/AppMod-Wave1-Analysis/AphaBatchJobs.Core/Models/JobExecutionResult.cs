namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the result of a scheduled job execution.
/// Contains status, message, and exit code to communicate job outcome.
/// </summary>
/// <remarks>
/// This model is used to standardize job execution results across all scheduled jobs.
/// Exit codes follow Unix convention: 0 = success, non-zero = failure.
/// Status values are constrained to: "Success", "Failed", or "Timeout".
/// </remarks>
public sealed class JobExecutionResult
{
    // Use constants for magic strings to ensure consistency and prevent typos
    private const string StatusSuccess = "Success";
    private const string StatusFailed = "Failed";
    private const string StatusTimeout = "Timeout";
    
    private const int ExitCodeSuccess = 0;
    private const int ExitCodeFailure = 1;
    private const int ExitCodeTimeout = 2;

    /// <summary>
    /// Gets or sets the execution status.
    /// Valid values: "Success", "Failed", "Timeout"
    /// </summary>
    /// <value>
    /// A string indicating the outcome of the job execution.
    /// "Success" - All steps completed successfully
    /// "Failed" - One or more steps failed during execution
    /// "Timeout" - A step exceeded the allowed execution time
    /// </value>
    public required string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable summary of the execution result.
    /// </summary>
    /// <value>
    /// A descriptive message providing details about the execution outcome.
    /// Examples:
    /// - "All 5 steps completed successfully"
    /// - "Failed at step 3: sp_RecreateYearData"
    /// - "Step 2 exceeded timeout of 300 seconds"
    /// </value>
    public required string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the exit code indicating the execution result.
    /// </summary>
    /// <value>
    /// An integer exit code following Unix conventions:
    /// 0 - Success: all operations completed successfully
    /// 1 - Failure: one or more operations failed
    /// 2 - Timeout: an operation exceeded the allowed execution time
    /// </value>
    public required int ExitCode { get; init; }

    /// <summary>
    /// Creates a successful job execution result.
    /// </summary>
    /// <param name="message">Optional success message. Defaults to "Job completed successfully"</param>
    /// <returns>A JobExecutionResult with Success status and exit code 0</returns>
    public static JobExecutionResult Success(string message = "Job completed successfully")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        
        return new JobExecutionResult
        {
            Status = StatusSuccess,
            Message = message,
            ExitCode = ExitCodeSuccess
        };
    }

    /// <summary>
    /// Creates a failed job execution result.
    /// </summary>
    /// <param name="message">Failure message describing what went wrong</param>
    /// <returns>A JobExecutionResult with Failed status and exit code 1</returns>
    public static JobExecutionResult Failure(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        
        return new JobExecutionResult
        {
            Status = StatusFailed,
            Message = message,
            ExitCode = ExitCodeFailure
        };
    }

    /// <summary>
    /// Creates a timeout job execution result.
    /// </summary>
    /// <param name="message">Timeout message describing which step timed out</param>
    /// <returns>A JobExecutionResult with Timeout status and exit code 2</returns>
    public static JobExecutionResult Timeout(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        
        return new JobExecutionResult
        {
            Status = StatusTimeout,
            Message = message,
            ExitCode = ExitCodeTimeout
        };
    }
}


**Key improvements made:**

1. **Constants for magic strings and numbers**: Added private constants for status strings and exit codes to prevent typos and ensure consistency throughout the class.

2. **Immutability with `init` accessors**: Changed `set` to `init` for all properties to make the object immutable after construction, which is a best practice for result/value objects in .NET 8.

3. **Required properties**: Added `required` modifier to properties to enforce initialization, ensuring objects are always in a valid state (available in C# 11/.NET 7+).

4. **Input validation**: Added `ArgumentException.ThrowIfNullOrWhiteSpace()` validation in factory methods to prevent creation of invalid results with null or empty messages (available in .NET 8).

5. **Maintained sealed class**: Kept the `sealed` modifier which is good for performance and prevents unintended inheritance.

These changes align with .NET 8 best practices for immutable data models, type safety, and defensive programming without adding new functionality.