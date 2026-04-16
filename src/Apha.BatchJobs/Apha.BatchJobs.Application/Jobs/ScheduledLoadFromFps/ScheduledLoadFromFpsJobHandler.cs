using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
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
        IOptions<ScheduledLoadFromFpsSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _planBuilder = planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var plan = _planBuilder.Build();
        var effectiveStepTimeout = _settings.StepTimeoutSeconds > 0
            ? _settings.StepTimeoutSeconds
            : DefaultStepTimeoutSeconds;

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

        foreach (var step in plan.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ExecuteStepSkeletonAsync(step, effectiveStepTimeout, cancellationToken);
        }

        _logger.LogInformation(
            "ScheduledLoadFromFps plan completed | ExecutedSteps={ExecutedSteps}",
            string.Join(",", plan.Steps.Select(static s => s.ToString())));
    }

    private async Task ExecuteStepSkeletonAsync(
        ScheduledLoadFromFpsStep step,
        int stepTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(stepTimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        _logger.LogInformation("Executing structured step {StepName}", step);

        try
        {
            // Intentional no-op placeholder: DB wiring lands in next phase.
            await Task.CompletedTask.WaitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Step '{step}' exceeded timeout of {stepTimeoutSeconds} seconds.");
        }
    }
}
