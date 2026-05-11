namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 2 — Rebuilds <c>fps.fpsyeartotals</c> from source views.</summary>
/// <remarks>Replaces <c>sp_createFPSTotals</c>.</remarks>
public sealed class CreateFpsTotalsStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public CreateFpsTotalsStep(string sql) => _sql = sql;

    public override string StepName => "CreateFpsTotals";
    protected override string SqlText => _sql;
}
