namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 10 — Populates <c>fps.projectmonth2</c> with single-month cost/invoice/milestone data.
/// </summary>
/// <remarks>Replaces <c>sp_qryJobMonth_Single</c>. No parameters required.</remarks>
public sealed class CreateProjectMonthSingleStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public CreateProjectMonthSingleStep(string sql) => _sql = sql;

    public override string StepName => "CreateProjectMonthSingle";
    protected override string SqlText => _sql;
}
