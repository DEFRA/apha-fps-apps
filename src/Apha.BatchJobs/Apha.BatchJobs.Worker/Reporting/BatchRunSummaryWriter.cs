using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Lifecycle;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Worker.Reporting;

/// <summary>
/// Writes the one structured summary line for a worker invocation, via <see cref="ILogger{TCategoryName}"/> only.
/// Never re-logs the exception — that already happened in <c>JobOrchestrator</c> or <c>BatchWorkerRunner</c>.
/// A failure line is prefixed with its <c>[{ErrorType}]</c> marker, so every failed run reaches the
/// CloudWatch metric filters, including failures raised before the orchestrator logs one itself.
/// </summary>
public sealed class BatchRunSummaryWriter : IBatchRunSummaryWriter
{
    private readonly ILogger<BatchRunSummaryWriter> _logger;

    public BatchRunSummaryWriter(ILogger<BatchRunSummaryWriter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private const string SummaryTemplate =
        "Run completed | Outcome={Outcome} | FailureCategory={FailureCategory} | ExitCode={ExitCode} | Message={Message} | JobName={JobName} | JobQueueId={JobQueueId} | ExecutionId={ExecutionId} | JobExecutionId={JobExecutionId} | RunMode={RunMode} | DurationMs={DurationMs}";

    public void WriteSummary(BatchExecutionResult result, TimeSpan duration)
    {
        if (result.Outcome == BatchRunOutcome.Failure)
        {
            _logger.LogError(
                "[{ErrorType}] " + SummaryTemplate,
                result.ErrorType ?? BatchExceptionMarkers.General,
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
            return;
        }

        var logLevel = result.Outcome == BatchRunOutcome.Success ? LogLevel.Information : LogLevel.Warning;

        _logger.Log(
            logLevel,
            SummaryTemplate,
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
        // Category-to-message mapping lives in BatchFailureMessageProvider — shared with
        // JobOrchestrator's job_queue.errormessage write, so the same failure produces the same
        // friendly text in both the CloudWatch log line and the grid.
        { Outcome: BatchRunOutcome.Failure, FailureCategory: { } category } =>
            BatchFailureMessageProvider.GetHumanReadableMessage(category),
        _ => "Job failed with a business or runtime exception."
    };
}
