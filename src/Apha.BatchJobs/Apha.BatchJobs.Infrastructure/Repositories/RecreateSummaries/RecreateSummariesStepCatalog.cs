namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Incremental LINQ catalog: steps 1-7 use LINQ implementations and remaining
/// steps continue to use SQL adapters until fully migrated.
/// </summary>
internal sealed class RecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public string ImplementationName => "DotNetLinq";

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, string triggeredBy) =>
    [
        new DeleteFpsTotalsStep(),
        new CreateFpsTotalsStep(),
        new InsertMissingProjectsStep(),
        new DeleteTimeCostCalcsStep(),
        new CreateTimeCostCalcsStep(),
        new DeleteProjectMonthCaseworkStep(),
        new CreateProjectMonthCaseworkStep(),
        new DeleteProjectMonthFinalStep(),
        new DeleteProjectMonth2Step(),
        new CreateProjectMonthSingleStep(),
        new DeleteProjectMonth3Step(),
        new CreateProjectMonthCumulativeStep(),
        new CreateProjectMonthFinalStep(month),
        new LogRecreateSummariesStep(month, triggeredBy),
    ];

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month) =>
    [
        new RefreshPeriodMoStep(month),
        new RefreshPeriodPscStep(month),
        new RefreshPeriodTccStep(month),
    ];
}
