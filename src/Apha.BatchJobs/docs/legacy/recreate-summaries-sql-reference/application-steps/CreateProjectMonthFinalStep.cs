using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 13 — Populates <c>fps.projectmonthfinal</c> with the final monthly view
/// combining single-month and cumulative data up to the supplied month.
/// </summary>
/// <remarks>
/// Replaces <c>sp_qryJobMonth_Final @Month</c>.
/// Passes <c>@month</c> as a parameter to the SQL.
/// </remarks>
public sealed class CreateProjectMonthFinalStep : RecreateSummariesStepBase
{
    private readonly string _sql;
    private readonly int _month;

    public CreateProjectMonthFinalStep(string sql, int month)
    {
        _sql = sql;
        _month = month;
    }

    public override string StepName => "CreateProjectMonthFinal";
    protected override string SqlText => _sql;

    protected override Task BuildCommandAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        command.Parameters.AddWithValue("month", _month);
        return Task.CompletedTask;
    }
}
