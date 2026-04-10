namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Immutable record representing the outcome of a job execution.
/// Contains status information, descriptive message, and exit code for process termination.
/// </summary>
/// <param name="Status">The execution status as a string representation</param>
/// <param name="Message">Descriptive message about the execution outcome</param>
/// <param name="ExitCode">Integer exit code to be returned to the operating system (0 for success, non-zero for failures)</param>
public sealed record JobExecutionResult(string Status, string Message, int ExitCode)
{
    // Best Practice: Add validation to ensure data integrity
    // Prevent null values which could cause runtime issues in ECS Fargate environments
    public string Status { get; init; } = Status ?? throw new ArgumentNullException(nameof(Status));
    public string Message { get; init; } = Message ?? throw new ArgumentNullException(nameof(Message));
    
    // Best Practice: Add factory methods for common scenarios to ensure consistency
    // This is particularly useful in batch job scenarios running on ECS Fargate
    public static JobExecutionResult Success(string message = "Job completed successfully") 
        => new(nameof(Success), message, 0);
    
    public static JobExecutionResult Failure(string message, int exitCode = 1) 
        => new(nameof(Failure), message, exitCode);
}


**Key improvements made:**

1. **Sealed modifier**: Added `sealed` keyword to prevent inheritance, which is a best practice for records that represent data contracts
2. **Null validation**: Added explicit property initializers with null checks to prevent null reference exceptions in containerized environments
3. **Factory methods**: Added `Success()` and `Failure()` static factory methods for consistent object creation across batch jobs
4. **Exit code defaults**: Ensured proper exit code handling (0 for success, non-zero for failures) which is critical for ECS Fargate task completion status
5. **Maintained immutability**: Kept the record immutable using `init` accessors, which is important for thread-safe operations in concurrent batch processing scenarios