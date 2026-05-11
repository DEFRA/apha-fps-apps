namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 11 — Deletes all rows from <c>fps.projectmonth3</c>.</summary>
/// <remarks>Replaces <c>sp_DeleteProjectMonth3</c>.</remarks>
public sealed class DeleteProjectMonth3Step : RecreateSummariesStepBase
{
    private readonly string _sql;

    public DeleteProjectMonth3Step(string sql) => _sql = sql;

    public override string StepName => "DeleteProjectMonth3";
    protected override string SqlText => _sql;
}
