namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Incremental LINQ catalog: steps 1-7 use LINQ implementations and remaining
/// steps continue to use SQL adapters until fully migrated.
/// </summary>
internal sealed class LinqRecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public string ImplementationName => "DotNetLinq";

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, string triggeredBy) =>
    [
        new LinqDeleteFpsTotalsStep(),
        new LinqCreateFpsTotalsStep(),
        new LinqInsertMissingProjectsStep(),
        new LinqDeleteTimeCostCalcsStep(),
        new LinqCreateTimeCostCalcsStep(),
        new LinqDeleteProjectMonthCaseworkStep(),
        new LinqCreateProjectMonthCaseworkStep(),
        new LinqDeleteProjectMonthFinalStep(),
        new LinqDeleteProjectMonth2Step(),
        new LinqCreateProjectMonthSingleStep(),
        new LinqDeleteProjectMonth3Step(),
        new LinqCreateProjectMonthCumulativeStep(),
        new LinqCreateProjectMonthFinalStep(month),
        new LinqLogRecreateSummariesStep(month, triggeredBy),
    ];

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month) =>
    [
        new LinqRefreshPeriodMoStep(month),
        new LinqRefreshPeriodPscStep(month),
        new LinqRefreshPeriodTccStep(month),
    ];
}
