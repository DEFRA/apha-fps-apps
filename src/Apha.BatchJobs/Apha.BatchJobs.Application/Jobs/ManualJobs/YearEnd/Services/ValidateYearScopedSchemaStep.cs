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
    /// Year End performs no DDL. Every table that needs a target-year partition — the 38 Table 23
    /// business participants plus the year-scoped configuration dependencies
    /// (fps.tblsettings/fps.tlkpmonthhours) — must already have it attached before any
    /// business-data mutation begins. Partition provisioning is an external DB/DBA prerequisite;
    /// this only validates.
    /// </summary>
    private static async Task ValidateTargetYearPartitionsAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        var notPartitioned = new List<string>();
        var missingPartition = new List<string>();

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            if (entry.Role is not (YearEndTableRole.YearScopedBusinessParticipant or YearEndTableRole.YearScopedConfigurationDependency))
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

            if (!await YearEndSqlHelpers.IsPartitionAttachedForYearAsync(connection, transaction, entry.Schema, entry.TableName, targetYear, cancellationToken))
            {
                missingPartition.Add(qualifiedName);
            }
        }

        if (notPartitioned.Count == 0 && missingPartition.Count == 0)
        {
            return;
        }

        var sections = new List<string>();
        if (notPartitioned.Count > 0)
        {
            sections.Add($"Not partitioned as expected: {string.Join(", ", notPartitioned)}.");
        }

        if (missingPartition.Count > 0)
        {
            sections.Add($"Missing target-year ({targetYear}) partition: {string.Join(", ", missingPartition)}.");
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
