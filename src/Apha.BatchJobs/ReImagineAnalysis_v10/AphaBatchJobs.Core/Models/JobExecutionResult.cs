namespace AphaBatchJobs.Core.Models;

/// <summary>
/// Represents the result of a job execution containing status information, 
/// a descriptive message, and an exit code for process termination.
/// </summary>
/// <param name="Status">The status of the job execution (e.g., "Success", "Failed", "PartialSuccess", "Skipped")</param>
/// <param name="Message">A descriptive message providing details about the job execution result</param>
/// <param name="ExitCode">The integer exit code to be returned to the operating system (0 for success, non-zero for failures)</param>
public sealed record JobExecutionResult(string Status, string Message, int ExitCode)
{
    /// <summary>
    /// Gets the status of the job execution.
    /// </summary>
    public string Status { get; init; } = Status ?? throw new ArgumentNullException(nameof(Status));

    /// <summary>
    /// Gets a descriptive message providing details about the job execution result.
    /// </summary>
    public string Message { get; init; } = Message ?? throw new ArgumentNullException(nameof(Message));

    /// <summary>
    /// Gets the integer exit code to be returned to the operating system.
    /// </summary>
    public int ExitCode { get; init; } = ExitCode;
}
