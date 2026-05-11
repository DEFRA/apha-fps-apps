using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 16 (conditional) — Refreshes <c>fps.period_proj_subcontract</c> for the given period.
/// Only executed when <c>tblperiod.periodlocked = 0</c>.
/// </summary>
/// <remarks>Replaces <c>usp_Refresh_Period_PSC @month</c>.</remarks>
public sealed class RefreshPeriodPscStep : RecreateSummariesStepBase
{
    private readonly string _sql;
    private readonly int _month;

    public RefreshPeriodPscStep(string sql, int month)
    {
        _sql = sql;
        _month = month;
    }

    public override string StepName => "RefreshPeriodPsc";
    protected override string SqlText => _sql;

    protected override Task BuildCommandAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        command.Parameters.AddWithValue("period", _month);
        return Task.CompletedTask;
    }
}
