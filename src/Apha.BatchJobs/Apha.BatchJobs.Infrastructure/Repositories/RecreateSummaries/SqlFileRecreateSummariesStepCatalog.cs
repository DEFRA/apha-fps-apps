using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Builds RecreateSummaries steps using external SQL script files.
/// </summary>
internal sealed class SqlFileRecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public string ImplementationName => "SqlFiles";

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, string triggeredBy) =>
    [
        Wrap(new DeleteFpsTotalsStep(SqlLoader.Load("01_delete_fps_totals.sql"))),
        Wrap(new CreateFpsTotalsStep(SqlLoader.Load("02_create_fps_totals.sql"))),
        Wrap(new InsertMissingProjectsStep(SqlLoader.Load("03_insert_missing_projects.sql"))),
        Wrap(new DeleteTimeCostCalcsStep(SqlLoader.Load("04_delete_time_cost_calcs.sql"))),
        Wrap(new CreateTimeCostCalcsStep(SqlLoader.Load("05_create_time_cost_calcs.sql"))),
        Wrap(new DeleteProjectMonthCaseworkStep(SqlLoader.Load("06_delete_project_month_casework.sql"))),
        Wrap(new CreateProjectMonthCaseworkStep(SqlLoader.Load("07_create_project_month_casework.sql"))),
        Wrap(new DeleteProjectMonthFinalStep(SqlLoader.Load("08_delete_project_month_final.sql"))),
        Wrap(new DeleteProjectMonth2Step(SqlLoader.Load("09_delete_project_month2.sql"))),
        Wrap(new CreateProjectMonthSingleStep(SqlLoader.Load("10_create_project_month_single.sql"))),
        Wrap(new DeleteProjectMonth3Step(SqlLoader.Load("11_delete_project_month3.sql"))),
        Wrap(new CreateProjectMonthCumulativeStep(SqlLoader.Load("12_create_project_month_cumulative.sql"))),
        Wrap(new CreateProjectMonthFinalStep(SqlLoader.Load("13_create_project_month_final.sql"), month)),
        Wrap(new LogRecreateSummariesStep(SqlLoader.Load("14_log_recreate_summaries.sql"), month, triggeredBy)),
    ];

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month) =>
    [
        Wrap(new RefreshPeriodMoStep(SqlLoader.Load("15_refresh_period_mo.sql"), month)),
        Wrap(new RefreshPeriodPscStep(SqlLoader.Load("16_refresh_period_psc.sql"), month)),
        Wrap(new RefreshPeriodTccStep(SqlLoader.Load("17_refresh_period_tcc.sql"), month)),
    ];

    private static IRecreateSummariesExecutionStep Wrap(IRecreateSummariesStep step)
        => new SqlRecreateSummariesExecutionStepAdapter(step);
}
