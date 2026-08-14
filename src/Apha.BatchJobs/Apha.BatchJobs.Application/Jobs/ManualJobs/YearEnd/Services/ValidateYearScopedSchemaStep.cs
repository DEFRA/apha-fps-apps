using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Validates that Year End year-control metadata exists and contains the current year row.
/// </summary>
public sealed class ValidateYearScopedSchemaStep : IYearEndDataSetupStep
{
    private readonly ILogger<ValidateYearScopedSchemaStep> _logger;

    public ValidateYearScopedSchemaStep(ILogger<ValidateYearScopedSchemaStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ValidateYearScopedSchemaStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before schema validation.");
        }

        if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, "fps", "tblyearmaster", cancellationToken))
        {
            throw new InvalidOperationException("Required table fps.tblyearmaster was not found. Year End cannot continue.");
        }

        var requiredColumns = new[] { "fpsyear", "fpsyearcode", "yearstatus", "active" };
        foreach (var columnName in requiredColumns)
        {
            if (!await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, "fps", "tblyearmaster", columnName, cancellationToken))
            {
                throw new InvalidOperationException($"Required column fps.tblyearmaster.{columnName} was not found. Year End cannot continue.");
            }
        }

        var currentYearExists = await RowExistsByYearAsync(connection, transaction, context.CurrentFpsYear.Value, cancellationToken);

        if (!currentYearExists)
        {
            throw new InvalidOperationException(
                $"Current year {context.CurrentFpsYear.Value} does not exist in fps.tblyearmaster. Year End cannot continue.");
        }

        await ValidateTargetYearPartitionsAsync(connection, transaction, context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd schema validation succeeded | CorrelationId={CorrelationId} | CurrentFpsYear={CurrentFpsYear} | TargetFpsYear={TargetFpsYear}",
            context.CorrelationId,
            context.CurrentFpsYear,
            context.TargetFpsYear);

        return context;
    }

    /// <summary>
    /// Year End performs no DDL. <c>fpsyear</c> is the authoritative business-year discriminator —
    /// the physical partition a row lands in is a storage implementation detail, not a business
    /// concept Year End enforces. Every year-scoped table — the 38 Table 23 business participants,
    /// the 3 year-scoped configuration dependencies, and the 21
    /// <see cref="YearEndTableRole.YearScopedTargetMustBeEmpty"/> tables (62 total) — must already
    /// have a routing destination for the target year before any business-data mutation begins: an
    /// explicit <c>FOR VALUES IN (targetYear)</c> partition, or an attached <c>DEFAULT</c>
    /// partition (rows routed via <c>DEFAULT</c> remain correctly discriminated by their
    /// <c>fpsyear</c> column value regardless of physical location). Either is a legitimate,
    /// DDL-free destination; this only validates routability, never creates partitions. Rows
    /// routed through <c>DEFAULT</c> do lose partition pruning versus an explicit per-year
    /// partition — worth keeping in mind for high-volume/continuously-written tables (e.g.
    /// <c>timecodevalid</c>, <c>projectmonth2/3</c>) that other processes such as
    /// <c>RecreateSummaries</c> keep writing to all year, not just during Year End's own copy step
    /// — but that is a performance consideration for whoever provisions partitions, not a Year End
    /// correctness gate.
    /// </summary>
    private static async Task ValidateTargetYearPartitionsAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        var notPartitioned = new List<string>();
        var unroutable = new List<string>();

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            if (entry.Role is not (YearEndTableRole.YearScopedBusinessParticipant
                or YearEndTableRole.YearScopedConfigurationDependency
                or YearEndTableRole.YearScopedTargetMustBeEmpty))
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var qualifiedName = $"{entry.Schema}.{entry.TableName}";

            if (!await YearEndSqlHelpers.IsPartitionedTableAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken))
            {
                notPartitioned.Add(qualifiedName);
                continue;
            }

            var hasExplicitPartition = await YearEndSqlHelpers.IsPartitionAttachedForYearAsync(connection, transaction, entry.Schema, entry.TableName, targetYear, cancellationToken);
            var hasDefaultPartition = !hasExplicitPartition
                && await YearEndSqlHelpers.IsDefaultPartitionAttachedAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken);

            if (!hasExplicitPartition && !hasDefaultPartition)
            {
                unroutable.Add(qualifiedName);
            }
        }

        if (notPartitioned.Count == 0 && unroutable.Count == 0)
        {
            return;
        }

        var sections = new List<string>();
        if (notPartitioned.Count > 0)
        {
            sections.Add($"Not partitioned as expected: {string.Join(", ", notPartitioned)}.");
        }

        if (unroutable.Count > 0)
        {
            sections.Add($"No routing destination (explicit or DEFAULT partition) for target year ({targetYear}): {string.Join(", ", unroutable)}.");
        }

        throw new InvalidOperationException(
            $"Year End target-year partition validation failed. {string.Join(" ", sections)} " +
            "Partition creation is an external DB/DBA prerequisite; Year End performs no DDL.");
    }

    private static async Task<bool> RowExistsByYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT EXISTS (
                SELECT 1
                FROM fps.tblyearmaster ym
                WHERE ym.fpsyear = @fpsyear
            );");

        YearEndSqlHelpers.AddParameter(command, "fpsyear", fpsYear);

        return await YearEndSqlHelpers.ExecuteBooleanAsync(command, cancellationToken);
    }
}
