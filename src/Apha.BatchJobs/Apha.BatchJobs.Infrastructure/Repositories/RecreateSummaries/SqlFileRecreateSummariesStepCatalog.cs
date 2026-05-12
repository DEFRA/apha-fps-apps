using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Builds RecreateSummaries steps using external SQL script files.
/// </summary>
internal sealed class SqlFileRecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public string ImplementationName => "SqlFiles";

    public IReadOnlyList<IRecreateSummariesStep> BuildMandatorySteps(int month, string triggeredBy) =>
    [
        new DeleteFpsTotalsStep(SqlLoader.Load("01_delete_fps_totals.sql")),
        new CreateFpsTotalsStep(SqlLoader.Load("02_create_fps_totals.sql")),
        new InsertMissingProjectsStep(SqlLoader.Load("03_insert_missing_projects.sql")),
        new DeleteTimeCostCalcsStep(SqlLoader.Load("04_delete_time_cost_calcs.sql")),
        new CreateTimeCostCalcsStep(SqlLoader.Load("05_create_time_cost_calcs.sql")),
        new DeleteProjectMonthCaseworkStep(SqlLoader.Load("06_delete_project_month_casework.sql")),
        new CreateProjectMonthCaseworkStep(SqlLoader.Load("07_create_project_month_casework.sql")),
        new DeleteProjectMonthFinalStep(SqlLoader.Load("08_delete_project_month_final.sql")),
        new DeleteProjectMonth2Step(SqlLoader.Load("09_delete_project_month2.sql")),
        new CreateProjectMonthSingleStep(SqlLoader.Load("10_create_project_month_single.sql")),
        new DeleteProjectMonth3Step(SqlLoader.Load("11_delete_project_month3.sql")),
        new CreateProjectMonthCumulativeStep(SqlLoader.Load("12_create_project_month_cumulative.sql")),
        new CreateProjectMonthFinalStep(SqlLoader.Load("13_create_project_month_final.sql"), month),
        new LogRecreateSummariesStep(SqlLoader.Load("14_log_recreate_summaries.sql"), month, triggeredBy),
    ];

    public IReadOnlyList<IRecreateSummariesStep> BuildRefreshSteps(int month) =>
    [
        new RefreshPeriodMoStep(SqlLoader.Load("15_refresh_period_mo.sql"), month),
        new RefreshPeriodPscStep(SqlLoader.Load("16_refresh_period_psc.sql"), month),
        new RefreshPeriodTccStep(SqlLoader.Load("17_refresh_period_tcc.sql"), month),
    ];
}
