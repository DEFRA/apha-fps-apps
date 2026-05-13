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
        new SqlRecreateSummariesExecutionStepAdapter(new Application.Jobs.ScheduledJobs.RecreateSummaries.Steps.RefreshPeriodMoStep(SqlLoader.Load("15_refresh_period_mo.sql"), month)),
        new SqlRecreateSummariesExecutionStepAdapter(new Application.Jobs.ScheduledJobs.RecreateSummaries.Steps.RefreshPeriodPscStep(SqlLoader.Load("16_refresh_period_psc.sql"), month)),
        new SqlRecreateSummariesExecutionStepAdapter(new Application.Jobs.ScheduledJobs.RecreateSummaries.Steps.RefreshPeriodTccStep(SqlLoader.Load("17_refresh_period_tcc.sql"), month)),
    ];
}
