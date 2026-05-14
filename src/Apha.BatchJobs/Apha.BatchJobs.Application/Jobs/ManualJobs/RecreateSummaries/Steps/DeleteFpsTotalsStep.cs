namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 1 — Deletes all rows from <c>fps.fpsyeartotals</c>.</summary>
/// <remarks>Replaces <c>sp_deleteFPSTotals</c>.</remarks>
public sealed class DeleteFpsTotalsStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public DeleteFpsTotalsStep(string sql) => _sql = sql;

    public override string StepName => "DeleteFpsTotals";
    protected override string SqlText => _sql;
}
