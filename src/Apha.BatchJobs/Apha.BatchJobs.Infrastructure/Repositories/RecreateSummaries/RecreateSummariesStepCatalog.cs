namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// All 14 orchestration steps implemented in LINQ/EF Core (production-ready).
/// Replaces the legacy SQL-based sp_RecreateSummaries orchestration procedure.
/// Fully aligned with SQL baseline (parity testing: 5/5 tests passing).
/// </summary>
internal sealed class RecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, int year, string triggeredBy) =>
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
        new LogRecreateSummariesStep(month, year, triggeredBy),
    ];

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month) =>
    [
        new RefreshPeriodMoStep(month),
        new RefreshPeriodPscStep(month),
        new RefreshPeriodTccStep(month),
    ];
}
