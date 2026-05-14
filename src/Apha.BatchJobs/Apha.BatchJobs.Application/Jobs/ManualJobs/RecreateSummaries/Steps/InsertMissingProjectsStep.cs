using Apha.BatchJobs.Domain.Enums;
using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

/// <summary>
/// Step 3 — Inserts missing rows into <c>fps.projectmonth</c> for all 12 months.
/// </summary>
/// <remarks>
/// Replaces <c>sp_InsertMissingProjects</c>.
/// The legacy procedure used a SQL Server WHILE loop (months 1–12).
/// Per the conversion principle, the loop moves to C#; the SQL body executes
/// once per iteration, exactly preserving the legacy row-by-row behaviour.
/// </remarks>
public sealed class InsertMissingProjectsStep : IRecreateSummariesStep
{
    private readonly string _sql;

    // This step does not use the month parameter from the job context;
    // it loops through all 12 calendar months regardless of the triggered month.
    public InsertMissingProjectsStep(string sql) =>
        _sql = sql;

    // Constructor overload accepted by the orchestrator — month parameter ignored at this step.
    public InsertMissingProjectsStep(string sql, int _) : this(sql) { }

    public string StepName => "InsertMissingProjects";

    public async Task<StepResult> ExecuteAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        var totalRowsAffected = 0;

        try
        {
            // C# replaces the WHILE loop — execute the INSERT body for each month 1–12
            for (var month = 1; month <= 12; month++)
            {
                await using var cmd = new NpgsqlCommand(_sql, connection);
                cmd.Parameters.AddWithValue("month", month);
                totalRowsAffected += await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            return new StepResult(StepName, totalRowsAffected, start, DateTime.UtcNow, StepStatus.Success);
        }
        catch (Exception ex)
        {
            return new StepResult(StepName, totalRowsAffected, start, DateTime.UtcNow, StepStatus.Failed, ex.Message);
        }
    }
}
