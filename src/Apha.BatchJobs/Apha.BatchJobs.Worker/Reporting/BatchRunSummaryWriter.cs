using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Lifecycle;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Worker.Reporting;

/// <summary>
/// Writes the one structured summary line for a worker invocation, via <see cref="ILogger{TCategoryName}"/> only.
/// Never re-logs the exception — that already happened in <c>JobOrchestrator</c> or <c>BatchWorkerRunner</c>.
/// </summary>
public sealed class BatchRunSummaryWriter : IBatchRunSummaryWriter
{
    private readonly ILogger<BatchRunSummaryWriter> _logger;

    public BatchRunSummaryWriter(ILogger<BatchRunSummaryWriter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void WriteSummary(BatchExecutionResult result, TimeSpan duration)
    {
        var logLevel = result.Outcome switch
        {
            BatchRunOutcome.Success => LogLevel.Information,
            BatchRunOutcome.Cancelled => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            "Run completed | Outcome={Outcome} | FailureCategory={FailureCategory} | ExitCode={ExitCode} | Message={Message} | JobName={JobName} | JobQueueId={JobQueueId} | ExecutionId={ExecutionId} | JobExecutionId={JobExecutionId} | RunMode={RunMode} | DurationMs={DurationMs}",
            result.Outcome,
            result.FailureCategory?.ToString() ?? "None",
            result.ExitCode,
            GenerateHumanReadableMessage(result),
            result.JobName ?? "Unknown",
            result.JobQueueId?.ToString() ?? "N/A",
            result.ExecutionId?.ToString() ?? "N/A",
            result.JobExecutionId?.ToString() ?? "N/A",
            result.RunMode?.ToString() ?? "Unknown",
            duration.TotalMilliseconds);
    }

    private static string GenerateHumanReadableMessage(BatchExecutionResult result) => result switch
    {
        { Outcome: BatchRunOutcome.Success } =>
            "Job completed successfully.",
        { Outcome: BatchRunOutcome.Cancelled, CancellationReason: ExecutionCancellationReason.HostShutdown } =>
            "Job execution was interrupted by host shutdown.",
        { Outcome: BatchRunOutcome.Cancelled, CancellationReason: ExecutionCancellationReason.Timeout } =>
            "Job failed because execution exceeded the configured overall timeout.",
        { Outcome: BatchRunOutcome.Cancelled } =>
            "Job execution was cancelled.",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.Sql } =>
            "Job failed due to a SQL error.",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.DependencyOutage } =>
            "Job failed due to a dependency outage (database unavailable, network timeout, etc.).",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.Configuration } =>
            "Job failed due to a configuration or validation error.",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.Concurrency } =>
            "Job failed because the distributed lock could not be acquired.",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.Email } =>
            "Job failed due to a business notification email error.",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.Timeout } =>
            "Job failed because execution exceeded the configured runtime timeout.",
        { Outcome: BatchRunOutcome.Failure, FailureCategory: BatchFailureCategory.Authorization } =>
            "Job failed due to an authorization error.",
        _ => "Job failed with a business or runtime exception."
    };
}
