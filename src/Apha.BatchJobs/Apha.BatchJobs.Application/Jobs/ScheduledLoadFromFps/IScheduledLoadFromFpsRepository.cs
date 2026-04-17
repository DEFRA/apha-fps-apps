namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;

using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;

/// <summary>
/// Persists run/step audit data and executes SQL operations for ScheduledLoadFromFps.
/// </summary>
public interface IScheduledLoadFromFpsRepository
{
    Task<Guid> StartRunAsync(string jobName, int fpsYear, string correlationId, CancellationToken cancellationToken);

    Task CompleteRunAsync(Guid runId, string finalStatus, CancellationToken cancellationToken);

    Task<Guid> StartStepAsync(Guid runId, ScheduledLoadFromFpsStep step, int stepSequence, CancellationToken cancellationToken);

    Task CompleteStepAsync(
        Guid stepRunId,
        string stepStatus,
        int? rowsAffected,
        string? errorMessage,
        CancellationToken cancellationToken);

    Task<int> RebuildYearTotalsAsync(int year, CancellationToken cancellationToken);

    Task<int> DeleteArchiveYearSliceAsync(int year, CancellationToken cancellationToken);

    Task<int> AddArchiveYearSliceAsync(int year, CancellationToken cancellationToken);

    Task<int> RefreshCurrentYearProjectAllAsync(int currentYear, CancellationToken cancellationToken);

    Task<IReadOnlyList<ScheduledLoadValidationAssertionResult>> RunCrossValidationAsync(
        Guid runId,
        ScheduledLoadFromFpsExecutionContext context,
        int expectedStepCount,
        CancellationToken cancellationToken);
}
