using Apha.BatchJobs.Domain.Enums;
using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

/// <summary>
/// Base class for RecreateSummaries steps.
/// Subclasses supply <see cref="StepName"/> and override
/// <see cref="BuildCommandAsync"/> to configure the <see cref="NpgsqlCommand"/>
/// (bind parameters, etc.) before execution.
/// The SQL text is loaded lazily from the <see cref="SqlText"/> property,
/// which subclasses must implement by returning the raw SQL string.
/// </summary>
public abstract class RecreateSummariesStepBase : IRecreateSummariesStep
{
    /// <inheritdoc />
    public abstract string StepName { get; }

    /// <summary>
    /// Returns the SQL text to execute.
    /// Implementations should return the content of the corresponding .sql file,
    /// injected at construction time.
    /// </summary>
    protected abstract string SqlText { get; }

    /// <inheritdoc />
    public async Task<StepResult> ExecuteAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        try
        {
            await using var cmd = new NpgsqlCommand(SqlText, connection);
            await BuildCommandAsync(cmd, cancellationToken);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return new StepResult(StepName, rowsAffected, start, DateTime.UtcNow, StepStatus.Success);
        }
        catch (Exception ex)
        {
            return new StepResult(StepName, 0, start, DateTime.UtcNow, StepStatus.Failed, ex.Message);
        }
    }

    /// <summary>
    /// Override to add parameters to the command before execution.
    /// Default implementation does nothing (for parameter-free steps).
    /// </summary>
    protected virtual Task BuildCommandAsync(NpgsqlCommand command, CancellationToken cancellationToken)
        => Task.CompletedTask;
}