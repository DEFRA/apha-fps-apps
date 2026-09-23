using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Data Setup.
/// </summary>
/// <remarks>
/// Runs all steps inside one <see cref="IYearEndDataSetupTransactionManager.ExecuteAsync"/> call so
/// they commit together, or roll back together if any step throws. Atomicity comes from the
/// repository sharing one scoped <c>BatchJobsDbContext</c> across all steps.
/// </remarks>
public sealed class YearEndDataSetupService : IYearEndDataSetupService
{
    private readonly IYearEndDataSetupTransactionManager _transactionManager;
    private readonly ILogger<YearEndDataSetupService> _logger;
    private readonly IReadOnlyList<IYearEndDataSetupStep> _steps;

    public YearEndDataSetupService(
        IEnumerable<IYearEndDataSetupStep> steps,
        IYearEndDataSetupTransactionManager transactionManager,
        ILogger<YearEndDataSetupService> logger)
    {
        _steps = steps?.ToList() ?? throw new ArgumentNullException(nameof(steps));
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "YearEndDataSetup service contract invoked | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            context.TargetFpsYear,
            context.CurrentFpsYear);

        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("Year End Data Setup has no registered execution steps.");
        }

        await _transactionManager.ExecuteAsync(async ct =>
        {
            for (var i = 0; i < _steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var step = _steps[i];
                var startedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "YearEndDataSetup step started | CorrelationId={CorrelationId} | StepIndex={StepIndex} | StepCount={StepCount} | StepName={StepName}",
                    context.CorrelationId,
                    i + 1,
                    _steps.Count,
                    step.Name);

                await step.ExecuteAsync(context, ct);

                var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
                _logger.LogInformation(
                    "YearEndDataSetup step completed | CorrelationId={CorrelationId} | StepIndex={StepIndex} | StepCount={StepCount} | StepName={StepName} | DurationMs={DurationMs}",
                    context.CorrelationId,
                    i + 1,
                    _steps.Count,
                    step.Name,
                    elapsedMs);
            }
        }, cancellationToken);

        _logger.LogInformation(
            "YearEndDataSetup pipeline completed | CorrelationId={CorrelationId}",
            context.CorrelationId);
    }
}
