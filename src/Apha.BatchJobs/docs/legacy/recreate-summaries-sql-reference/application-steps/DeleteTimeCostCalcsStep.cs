namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 4 — Deletes all rows from <c>fps.timecostcalcs</c>.</summary>
/// <remarks>Replaces <c>sp_deleteTimeCostCalcs</c>.</remarks>
public sealed class DeleteTimeCostCalcsStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public DeleteTimeCostCalcsStep(string sql) => _sql = sql;

    public override string StepName => "DeleteTimeCostCalcs";
    protected override string SqlText => _sql;
}
