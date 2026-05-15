namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 9 — Deletes all rows from <c>fps.projectmonth2</c>.</summary>
/// <remarks>Replaces <c>sp_deleteProjectMonth2</c>.</remarks>
public sealed class DeleteProjectMonth2Step : RecreateSummariesStepBase
{
    private readonly string _sql;

    public DeleteProjectMonth2Step(string sql) => _sql = sql;

    public override string StepName => "DeleteProjectMonth2";
    protected override string SqlText => _sql;
}
