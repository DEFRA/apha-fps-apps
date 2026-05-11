namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>Step 8 — Deletes all rows from <c>fps.projectmonthfinal</c>.</summary>
/// <remarks>Replaces <c>sp_DeleteProjectMonthFinal</c>.</remarks>
public sealed class DeleteProjectMonthFinalStep : RecreateSummariesStepBase
{
    private readonly string _sql;

    public DeleteProjectMonthFinalStep(string sql) => _sql = sql;

    public override string StepName => "DeleteProjectMonthFinal";
    protected override string SqlText => _sql;
}
