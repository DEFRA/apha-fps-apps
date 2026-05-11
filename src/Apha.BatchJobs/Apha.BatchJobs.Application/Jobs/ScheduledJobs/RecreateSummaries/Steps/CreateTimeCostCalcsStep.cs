namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 5 — Rebuilds <c>fps.timecostcalcs</c> from staff, grade, and time joins.</summary>
/// <remarks>Replaces <c>sp_CreateTimeCostCalcs</c>.</remarks>
public sealed class CreateTimeCostCalcsStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public CreateTimeCostCalcsStep(string sql) => _sql = sql;

    public override string StepName => "CreateTimeCostCalcs";
    protected override string SqlText => _sql;
}
