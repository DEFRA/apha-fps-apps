using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 15 (conditional) — Refreshes <c>fps.period_monthlyoutput</c> for the given period.
/// Only executed when <c>tblperiod.periodlocked = 0</c>.
/// </summary>
/// <remarks>Replaces <c>usp_Refresh_Period_MO @month</c>.</remarks>
public sealed class RefreshPeriodMoStep : RecreateSummariesStepBase
{
    private readonly string _sql;
    private readonly int _month;

    public RefreshPeriodMoStep(string sql, int month)
    {
        _sql = sql;
        _month = month;
    }

    public override string StepName => "RefreshPeriodMo";
    protected override string SqlText => _sql;

    protected override Task BuildCommandAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        command.Parameters.AddWithValue("period", _month);
        return Task.CompletedTask;
    }
}
