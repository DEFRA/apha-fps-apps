using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Contract for a single executable RecreateSummaries step.
/// This abstraction supports the LINQ/EF Core orchestration pipeline.
/// </summary>
public interface IRecreateSummariesExecutionStep
{
    /// <summary>Unique display name matching the legacy procedure name.</summary>
    string StepName { get; }

    /// <summary>
    /// Executes the step using the supplied execution context.
    /// </summary>
    Task<StepResult> ExecuteAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken = default);
}
