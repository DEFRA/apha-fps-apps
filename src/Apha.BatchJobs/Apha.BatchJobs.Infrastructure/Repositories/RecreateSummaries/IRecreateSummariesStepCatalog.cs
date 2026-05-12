using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Builds the ordered step list for RecreateSummaries execution.
/// </summary>
public interface IRecreateSummariesStepCatalog
{
    /// <summary>
    /// Human-readable implementation name for diagnostics.
    /// </summary>
    string ImplementationName { get; }

    /// <summary>
    /// Builds mandatory steps 1-14.
    /// </summary>
    IReadOnlyList<IRecreateSummariesStep> BuildMandatorySteps(int month, string triggeredBy);

    /// <summary>
    /// Builds conditional refresh steps 15-17.
    /// </summary>
    IReadOnlyList<IRecreateSummariesStep> BuildRefreshSteps(int month);
}
