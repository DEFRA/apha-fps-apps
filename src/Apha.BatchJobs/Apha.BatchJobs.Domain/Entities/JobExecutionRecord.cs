using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Domain.Entities;

/// <summary>
/// Represents an execution record for a batch job run.
/// </summary>
public sealed class JobExecutionRecord
{
    /// <summary>
    /// Unique identifier for this execution.
    /// </summary>
    public required int ExecutionId { get; set; }

    /// <summary>
    /// Name of the batch job.
    /// </summary>
    public required string JobName { get; set; }

    /// <summary>
    /// Unique run ID for this execution instance.
    /// </summary>
    public required string RunId { get; set; }

    /// <summary>
    /// Type of batch job.
    /// </summary>
    public required JobType JobType { get; set; }

    /// <summary>
    /// Execution mode (Scheduled or Manual).
    /// </summary>
    public required RunMode RunMode { get; set; }

    /// <summary>
    /// Current status of the job execution.
    /// </summary>
    public required JobStatus Status { get; set; }

    /// <summary>
    /// Timestamp when execution started.
    /// </summary>
    public required DateTime StartedAt { get; set; }

    /// <summary>
    /// Timestamp when execution completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Duration in seconds (calculated if completed).
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Number of records processed.
    /// </summary>
    public int? RecordsProcessed { get; set; }

    /// <summary>
    /// Number of records failed.
    /// </summary>
    public int? RecordsFailed { get; set; }

    /// <summary>
    /// Error message if execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace if execution failed.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Number of retry attempts made.
    /// </summary>
    public int RetryAttempts { get; set; }
}
