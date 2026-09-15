using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Worker.Lifecycle;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Apha.BatchJobs.Worker.Configuration;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Coordinates one worker invocation: resolve the request(s), run the orchestrator for each, and
/// write one summary per job. Usually resolves to a single job; a shared category trigger (see
/// <see cref="MonthlyScheduledNotificationJobs"/>) resolves to several, run in sequence within
/// this one invocation. HealthCheck never reaches this type — <c>Program.cs</c> short-circuits
/// before it.
/// </summary>
public sealed class BatchWorkerRunner : IBatchWorkerRunner
{
    private readonly BatchExecutionRequestResolver _requestResolver;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly IOptions<BatchRuntimeOptions> _runtimeOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly BatchFailureClassifier _failureClassifier;
    private readonly IBatchRunSummaryWriter _summaryWriter;
    private readonly ILogger<BatchWorkerRunner> _logger;

    public BatchWorkerRunner(
        BatchExecutionRequestResolver requestResolver,
        IHostApplicationLifetime hostLifetime,
        IOptions<BatchRuntimeOptions> runtimeOptions,
        IServiceProvider serviceProvider,
        BatchFailureClassifier failureClassifier,
        IBatchRunSummaryWriter summaryWriter,
        ILogger<BatchWorkerRunner> logger)
    {
        _requestResolver = requestResolver ?? throw new ArgumentNullException(nameof(requestResolver));
        _hostLifetime = hostLifetime ?? throw new ArgumentNullException(nameof(hostLifetime));
        _runtimeOptions = runtimeOptions ?? throw new ArgumentNullException(nameof(runtimeOptions));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _failureClassifier = failureClassifier ?? throw new ArgumentNullException(nameof(failureClassifier));
        _summaryWriter = summaryWriter ?? throw new ArgumentNullException(nameof(summaryWriter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> RunAsync()
    {
        IReadOnlyList<BatchExecutionRequest> requests;

        try
        {
            requests = _requestResolver.Resolve();
        }
        catch (Exception ex)
        {
            // First and only place this can be logged — resolution runs before any scope exists.
            var classification = _failureClassifier.Classify(ex);
            _logger.LogError(ex, "[{ErrorType}] Batch execution request could not be resolved: {ErrorMessage}", classification.ErrorType, ex.Message);

            var resolutionFailure = BatchExecutionResult.Failure(request: null, classification, ex);
            _summaryWriter.WriteSummary(resolutionFailure, TimeSpan.Zero);
            return resolutionFailure.ExitCode;
        }

        // Created before any execution scope so every job below — including each job of a
        // fanned-out category trigger — shares one cancellation boundary for the invocation.
        using var cancellationContext = new ExecutionCancellationContext(_hostLifetime, _runtimeOptions.Value.WorkerOverallTimeoutSeconds);

        var overallExitCode = BatchExitCodes.Success;

        foreach (var request in requests)
        {
            using var correlationScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["JobName"] = request.JobName,
                ["JobExecutionId"] = request.JobExecutionId,
                ["RunMode"] = request.RunMode.ToString(),
                ["RequestedBy"] = request.RequestedBy
            });

            var startedAt = DateTime.UtcNow;

            BatchExecutionResult result;
            try
            {
                await using var executionScope = _serviceProvider.CreateAsyncScope();
                var orchestrator = executionScope.ServiceProvider.GetRequiredService<IJobOrchestrator>();

                var jobResult = await orchestrator.RunAsync(
                    request.JobName,
                    request.RunMode,
                    request.JobExecutionId,
                    request.RequestedBy,
                    request.RequestedAtUtc,
                    request.ParametersJson,
                    cancellationContext.Token);

                result = BatchExecutionResult.Success(request, jobResult);
            }
            catch (OperationCanceledException)
            {
                result = BatchExecutionResult.Cancelled(request, cancellationContext.ClassifyCancellation());
            }
            catch (Exception ex)
            {
                // Already logged by JobOrchestrator.ThrowWithStructuredLog before it re-threw — don't log again.
                result = BatchExecutionResult.Failure(request, _failureClassifier.Classify(ex), ex);
            }

            _summaryWriter.WriteSummary(result, DateTime.UtcNow - startedAt);

            if (result.ExitCode != BatchExitCodes.Success && overallExitCode == BatchExitCodes.Success)
                overallExitCode = result.ExitCode;

            // Host shutdown or the overall timeout fired — remaining fanned-out jobs would just
            // cancel immediately too, so stop rather than attempt them. The invocation didn't run
            // every resolved job, so it must not report success even if every job that did run
            // (e.g. this one) succeeded.
            if (cancellationContext.Token.IsCancellationRequested)
            {
                if (overallExitCode == BatchExitCodes.Success)
                    overallExitCode = BatchExitCodes.Cancelled;
                break;
            }
        }

        return overallExitCode;
    }
}
