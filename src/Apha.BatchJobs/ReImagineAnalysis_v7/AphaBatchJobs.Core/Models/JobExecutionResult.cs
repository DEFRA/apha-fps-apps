namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the outcome of a job execution.
/// Immutable data structure for returning job results.
/// </summary>
/// <param name="Status">The status of the job execution (e.g., "Success", "Failed", "PartialSuccess", "Skipped")</param>
/// <param name="Message">A descriptive message about the job execution result</param>
/// <param name="ExitCode">The exit code representing the execution outcome (0 for success, non-zero for failures)</param>
public sealed record JobExecutionResult(string Status, string Message, int ExitCode)
{
    // Add validation to ensure data integrity
    public JobExecutionResult(string Status, string Message, int ExitCode) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Status, nameof(Status));
        ArgumentNullException.ThrowIfNull(Message, nameof(Message));
        
        this.Status = Status;
        this.Message = Message;
        this.ExitCode = ExitCode;
    }
    
    // Consider adding factory methods for common scenarios
    public static JobExecutionResult Success(string message = "Job completed successfully") 
        => new(Status: "Success", Message: message, ExitCode: 0);
    
    public static JobExecutionResult Failed(string message, int exitCode = 1) 
        => new(Status: "Failed", Message: message, ExitCode: exitCode);
    
    public static JobExecutionResult PartialSuccess(string message, int exitCode = 2) 
        => new(Status: "PartialSuccess", Message: message, ExitCode: exitCode);
    
    public static JobExecutionResult Skipped(string message) 
        => new(Status: "Skipped", Message: message, ExitCode: 0);
}


**Key improvements made:**

1. **Sealed modifier**: Added `sealed` to prevent inheritance, which is a best practice for records that represent data contracts
2. **Input validation**: Added parameter validation to ensure Status is not null/empty and Message is not null
3. **Factory methods**: Added static factory methods for common job execution results, improving code readability and reducing errors from manual string construction
4. **Explicit constructor**: Added an explicit constructor with validation while maintaining the positional record syntax
5. **Consistent naming**: Used named parameters in factory methods for clarity

These changes maintain the existing functionality while making the code more robust and idiomatic for .NET applications.