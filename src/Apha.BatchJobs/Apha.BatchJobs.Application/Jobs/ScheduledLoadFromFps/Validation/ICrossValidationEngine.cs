namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;

/// <summary>
/// Executes and persists ScheduledLoadFromFps cross-validation assertions.
/// </summary>
public interface ICrossValidationEngine
{
    Task<IReadOnlyList<ScheduledLoadValidationAssertionResult>> ExecuteAsync(
        Guid runId,
        ScheduledLoadFromFpsExecutionContext context,
        int expectedStepCount,
        CancellationToken cancellationToken);
}
