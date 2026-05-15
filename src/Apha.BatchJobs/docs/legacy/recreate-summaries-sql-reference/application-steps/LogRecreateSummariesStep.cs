using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 14 — Inserts an audit row into <c>fps.recreatesummaries_log</c>.
/// </summary>
/// <remarks>
/// Replaces <c>usp_LogRecreateSummaries @Month</c> and the inner call to
/// <c>sp_Get_SP_No</c>. The triggering user is supplied directly as a
/// constructor argument rather than reading <c>SYSTEM_USER</c>
/// from the database connection (Phase 7).
/// </remarks>
public sealed class LogRecreateSummariesStep : RecreateSummariesStepBase
{
    private readonly string _sql;
    private readonly int _month;
    private readonly string _triggeredBy;

    public LogRecreateSummariesStep(string sql, int month, string triggeredBy)
    {
        _sql = sql;
        _month = month;
        _triggeredBy = triggeredBy;
    }

    public override string StepName => "LogRecreateSummaries";
    protected override string SqlText => _sql;

    protected override Task BuildCommandAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        command.Parameters.AddWithValue("userId", _triggeredBy);
        command.Parameters.AddWithValue("month", _month);
        return Task.CompletedTask;
    }
}
