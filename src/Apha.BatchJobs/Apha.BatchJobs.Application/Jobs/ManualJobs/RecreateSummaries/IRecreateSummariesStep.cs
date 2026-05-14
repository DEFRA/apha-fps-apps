using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

/// <summary>
/// Contract for a single RecreateSummaries execution step.
/// Each implementation wraps exactly one legacy stored-procedure equivalent.
/// </summary>
public interface IRecreateSummariesStep
{
    /// <summary>Unique display name matching the legacy procedure name.</summary>
    string StepName { get; }

    /// <summary>
    /// Executes the step SQL on the supplied open connection.
    /// The connection must already be enrolled in the caller's transaction.
    /// </summary>
    Task<StepResult> ExecuteAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default);
}