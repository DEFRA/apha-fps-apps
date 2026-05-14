namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 7 — Rebuilds <c>fps.projectmonthcasework</c> from <c>qryprojectmonthcw</c>.</summary>
/// <remarks>Replaces <c>sp_CreateProjectMonthCasework</c>.</remarks>
public sealed class CreateProjectMonthCaseworkStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public CreateProjectMonthCaseworkStep(string sql) => _sql = sql;

    public override string StepName => "CreateProjectMonthCasework";
    protected override string SqlText => _sql;
}
