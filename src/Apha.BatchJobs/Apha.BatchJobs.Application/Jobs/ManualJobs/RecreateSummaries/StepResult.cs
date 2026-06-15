using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

/// <summary>
/// Captures the outcome of one RecreateSummaries execution step.
/// Collected by the handler and written to the job execution log.
/// </summary>
public sealed record StepResult(
    string StepName,
    int RowsAffected,
    DateTime StartTime,
    DateTime EndTime,
    StepStatus Status,
    string? ErrorMessage = null);
