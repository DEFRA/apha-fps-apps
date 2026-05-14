namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 12 — Populates <c>fps.projectmonth3</c> with cumulative period summaries.
/// </summary>
/// <remarks>Replaces <c>sp_qryJobMonthCum</c>. No parameters required.</remarks>
public sealed class CreateProjectMonthCumulativeStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public CreateProjectMonthCumulativeStep(string sql) => _sql = sql;

    public override string StepName => "CreateProjectMonthCumulative";
    protected override string SqlText => _sql;
}
