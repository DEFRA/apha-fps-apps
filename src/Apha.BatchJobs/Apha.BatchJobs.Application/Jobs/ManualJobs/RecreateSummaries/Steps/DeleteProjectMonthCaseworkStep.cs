namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 6 — Deletes all rows from <c>fps.projectmonthcasework</c>.</summary>
/// <remarks>Replaces <c>sp_DeleteProjectMonthCasework</c>.</remarks>
public sealed class DeleteProjectMonthCaseworkStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public DeleteProjectMonthCaseworkStep(string sql) => _sql = sql;

    public override string StepName => "DeleteProjectMonthCasework";
    protected override string SqlText => _sql;
}
