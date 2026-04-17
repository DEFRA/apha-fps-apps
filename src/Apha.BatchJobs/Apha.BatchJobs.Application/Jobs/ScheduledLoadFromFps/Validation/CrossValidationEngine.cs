using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;

/// <summary>
/// Executes repository-backed assertion checks and returns persisted validation outcomes.
/// </summary>
public sealed class CrossValidationEngine : ICrossValidationEngine
{
    private readonly IScheduledLoadFromFpsRepository _repository;
    private readonly ILogger<CrossValidationEngine> _logger;

    public CrossValidationEngine(
        IScheduledLoadFromFpsRepository repository,
        ILogger<CrossValidationEngine> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<ScheduledLoadValidationAssertionResult>> ExecuteAsync(
        Guid runId,
        ScheduledLoadFromFpsExecutionContext context,
        int expectedStepCount,
        CancellationToken cancellationToken)
    {
        var results = await _repository.RunCrossValidationAsync(runId, context, expectedStepCount, cancellationToken);
        var failed = results.Where(static r => !r.Passed).ToList();

        _logger.LogInformation(
            "ScheduledLoadFromFps validation completed | RunId={RunId} | TotalAssertions={TotalAssertions} | FailedAssertions={FailedAssertions}",
            runId,
            results.Count,
            failed.Count);

        if (failed.Count > 0)
        {
            _logger.LogWarning(
                "ScheduledLoadFromFps validation failures | RunId={RunId} | FailedCodes={FailedCodes}",
                runId,
                string.Join(",", failed.Select(static f => f.AssertionCode)));
        }

        return results;
    }
}
