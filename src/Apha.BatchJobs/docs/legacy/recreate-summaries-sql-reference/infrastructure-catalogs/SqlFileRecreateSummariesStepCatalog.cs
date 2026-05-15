using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;
using AppSteps = Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Builds RecreateSummaries steps using external SQL script files.
/// </summary>
internal sealed class SqlFileRecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public string ImplementationName => "SqlFiles";

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, string triggeredBy) =>
    [
        Wrap(new AppSteps.DeleteFpsTotalsStep(SqlLoader.Load("01_delete_fps_totals.sql"))),
        Wrap(new AppSteps.CreateFpsTotalsStep(SqlLoader.Load("02_create_fps_totals.sql"))),
        Wrap(new AppSteps.InsertMissingProjectsStep(SqlLoader.Load("03_insert_missing_projects.sql"))),
        Wrap(new AppSteps.DeleteTimeCostCalcsStep(SqlLoader.Load("04_delete_time_cost_calcs.sql"))),
        Wrap(new AppSteps.CreateTimeCostCalcsStep(SqlLoader.Load("05_create_time_cost_calcs.sql"))),
        Wrap(new AppSteps.DeleteProjectMonthCaseworkStep(SqlLoader.Load("06_delete_project_month_casework.sql"))),
        Wrap(new AppSteps.CreateProjectMonthCaseworkStep(SqlLoader.Load("07_create_project_month_casework.sql"))),
        Wrap(new AppSteps.DeleteProjectMonthFinalStep(SqlLoader.Load("08_delete_project_month_final.sql"))),
        Wrap(new AppSteps.DeleteProjectMonth2Step(SqlLoader.Load("09_delete_project_month2.sql"))),
        Wrap(new AppSteps.CreateProjectMonthSingleStep(SqlLoader.Load("10_create_project_month_single.sql"))),
        Wrap(new AppSteps.DeleteProjectMonth3Step(SqlLoader.Load("11_delete_project_month3.sql"))),
        Wrap(new AppSteps.CreateProjectMonthCumulativeStep(SqlLoader.Load("12_create_project_month_cumulative.sql"))),
        Wrap(new AppSteps.CreateProjectMonthFinalStep(SqlLoader.Load("13_create_project_month_final.sql"), month)),
        Wrap(new AppSteps.LogRecreateSummariesStep(SqlLoader.Load("14_log_recreate_summaries.sql"), month, triggeredBy)),
    ];

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month) =>
    [
        Wrap(new AppSteps.RefreshPeriodMoStep(SqlLoader.Load("15_refresh_period_mo.sql"), month)),
        Wrap(new AppSteps.RefreshPeriodPscStep(SqlLoader.Load("16_refresh_period_psc.sql"), month)),
        Wrap(new AppSteps.RefreshPeriodTccStep(SqlLoader.Load("17_refresh_period_tcc.sql"), month)),
    ];

    private static IRecreateSummariesExecutionStep Wrap(IRecreateSummariesStep step)
        => new SqlRecreateSummariesExecutionStepAdapter(step);
}
