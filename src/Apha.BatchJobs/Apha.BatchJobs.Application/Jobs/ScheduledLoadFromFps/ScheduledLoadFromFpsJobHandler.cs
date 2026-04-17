using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

/// <summary>
/// Foundation handler for the LoadFromFPS scheduled orchestration.
/// This class currently structures sequencing and branching only.
/// DB step execution will be plugged into this flow in the next phase.
/// </summary>
public sealed class ScheduledLoadFromFpsJobHandler : IBatchJob
{
    private const int DefaultStepTimeoutSeconds = 300;
    private readonly ILogger<ScheduledLoadFromFpsJobHandler> _logger;
    private readonly IScheduledLoadFromFpsPlanBuilder _planBuilder;
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ICorrelationService _correlationService;
    private readonly ICrossValidationEngine _crossValidationEngine;
    private readonly IReadOnlyDictionary<ScheduledLoadFromFpsStep, IScheduledLoadFromFpsStepHandler> _stepHandlers;
    private readonly ScheduledLoadFromFpsSettings _settings;

    /// <summary>
    /// Canonical job name resolved by the orchestrator/factory.
    /// </summary>
    public string Name => "ScheduledLoadFromFps";

    /// <summary>
    /// Explicit idempotency strategy declaration for this job.
    /// </summary>
    public string IdempotencyStrategy => "YearScopedRebuildWithDeterministicOrdering";

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledLoadFromFpsJobHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="planBuilder">Execution plan builder.</param>
    /// <param name="settings">Scheduled job settings.</param>
    public ScheduledLoadFromFpsJobHandler(
        ILogger<ScheduledLoadFromFpsJobHandler> logger,
        IScheduledLoadFromFpsPlanBuilder planBuilder,
        IScheduledLoadFromFpsRepository repository,
        ICorrelationService correlationService,
        ICrossValidationEngine crossValidationEngine,
        IEnumerable<IScheduledLoadFromFpsStepHandler> stepHandlers,
        IOptions<ScheduledLoadFromFpsSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _planBuilder = planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _crossValidationEngine = crossValidationEngine ?? throw new ArgumentNullException(nameof(crossValidationEngine));
        _stepHandlers = BuildStepHandlerMap(stepHandlers);
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var plan = _planBuilder.Build();
        var effectiveStepTimeout = _settings.StepTimeoutSeconds > 0
            ? _settings.StepTimeoutSeconds
            : DefaultStepTimeoutSeconds;
        var correlationId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();
        var runId = await _repository.StartRunAsync(Name, plan.Context.CurrentYear, correlationId, cancellationToken);
        var finalStatus = "Completed";

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CurrentMonth"] = plan.Context.CurrentMonth,
            ["CurrentYear"] = plan.Context.CurrentYear,
            ["PreviousYear"] = plan.Context.PreviousYear,
            ["CurrentYearCutoverMonth"] = plan.Context.CurrentYearCutoverMonth
        });

        _logger.LogInformation(
            "ScheduledLoadFromFps start | CurrentYear={CurrentYear} | PreviousYear={PreviousYear} | CurrentMonth={CurrentMonth} | StepTimeoutSeconds={StepTimeoutSeconds}",
            plan.Context.CurrentYear,
            plan.Context.PreviousYear,
            plan.Context.CurrentMonth,
            effectiveStepTimeout);

        try
        {
            var stepSequence = 1;
            foreach (var step in plan.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepRunId = await _repository.StartStepAsync(runId, step, stepSequence, cancellationToken);

                try
                {
                    var rowsAffected = await ExecuteStepAsync(step, plan.Context, effectiveStepTimeout, cancellationToken);
                    await _repository.CompleteStepAsync(stepRunId, "Completed", rowsAffected, null, cancellationToken);
                }
                catch (Exception ex)
                {
                    await _repository.CompleteStepAsync(stepRunId, "Failed", null, ex.Message, cancellationToken);
                    throw;
                }

                stepSequence++;
            }

            var validationResults = await _crossValidationEngine.ExecuteAsync(
                runId,
                plan.Context,
                plan.Steps.Count,
                cancellationToken);

            if (validationResults.Any(static r => !r.Passed))
            {
                finalStatus = "Failed";
                throw new InvalidOperationException("Cross-validation failed for one or more assertions.");
            }
        }
        catch (OperationCanceledException)
        {
            finalStatus = "Cancelled";
            throw;
        }
        catch
        {
            finalStatus = "Failed";
            throw;
        }
        finally
        {
            await _repository.CompleteRunAsync(runId, finalStatus, CancellationToken.None);
        }

        _logger.LogInformation(
            "ScheduledLoadFromFps plan completed | ExecutedSteps={ExecutedSteps}",
            string.Join(",", plan.Steps.Select(static s => s.ToString())));
    }

    private async Task<int> ExecuteStepAsync(
        ScheduledLoadFromFpsStep step,
        ScheduledLoadFromFpsExecutionContext context,
        int stepTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        if (!_stepHandlers.TryGetValue(step, out var handler))
        {
            throw new InvalidOperationException($"No step handler registered for step '{step}'.");
        }

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(stepTimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        _logger.LogInformation("Executing structured step {StepName}", step);

        try
        {
            return await handler.ExecuteAsync(context, linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Step '{step}' exceeded timeout of {stepTimeoutSeconds} seconds.");
        }
    }

    private static IReadOnlyDictionary<ScheduledLoadFromFpsStep, IScheduledLoadFromFpsStepHandler> BuildStepHandlerMap(
        IEnumerable<IScheduledLoadFromFpsStepHandler> stepHandlers)
    {
        if (stepHandlers == null)
        {
            throw new ArgumentNullException(nameof(stepHandlers));
        }

        var map = new Dictionary<ScheduledLoadFromFpsStep, IScheduledLoadFromFpsStepHandler>();
        foreach (var handler in stepHandlers)
        {
            if (!map.TryAdd(handler.Step, handler))
            {
                throw new InvalidOperationException($"Duplicate step handler registration for '{handler.Step}'.");
            }
        }

        return map;
    }
}
