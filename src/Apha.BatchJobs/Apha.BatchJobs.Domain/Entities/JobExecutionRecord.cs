using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Domain.Entities;

/// <summary>Represents an execution record for a batch job run.</summary>
public sealed class JobExecutionRecord
{
    /// <summary>Unique identifier for this execution.</summary>
    public required int ExecutionId { get; set; }

    /// <summary>Name of the batch job.</summary>
    public required string JobName { get; set; }

    /// <summary>External UUID supplied by caller/API for status polling and correlation.</summary>
    public required Guid JobExecutionId { get; set; }

    /// <summary>Internal UUID for this worker-owned job queue entry.</summary>
    public required Guid JobQueueId { get; set; }

    /// <summary>User ID or system identifier requesting the job execution.</summary>
    public required string UserId { get; set; }

    /// <summary>Type of batch job.</summary>
    public required JobType JobType { get; set; }

    /// <summary>Execution mode (Scheduled or Manual).</summary>
    public required RunMode RunMode { get; set; }

    /// <summary>Current status of the job execution.</summary>
    public required JobStatus Status { get; set; }

    /// <summary>Timestamp when execution started.</summary>
    public required DateTime StartedAt { get; set; }

    /// <summary>Timestamp when trigger was accepted by API and published to worker.</summary>
    public DateTime? RequestedAtUtc { get; set; }

    /// <summary>FPS year associated with this execution when applicable (for example RecreateSummaries).</summary>
    public int? FpsYear { get; set; }

    /// <summary>
    /// The year a Year End Data Setup/CutOver (or other approval-based) request is preparing —
    /// distinct from <see cref="FpsYear"/>, which is the request's own current/open year.
    /// </summary>
    public int? TargetFpsYear { get; set; }

    /// <summary>Timestamp when execution completed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Duration in seconds (calculated if completed).</summary>
    public int? DurationSeconds { get; set; }

    /// <summary>Number of records processed.</summary>
    public int? RecordsProcessed { get; set; }

    /// <summary>Number of records failed.</summary>
    public int? RecordsFailed { get; set; }

    /// <summary>Error message if execution failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Stack trace if execution failed.</summary>
    public string? StackTrace { get; set; }

    /// <summary>Number of retry attempts made.</summary>
    public int RetryAttempts { get; set; }
}
