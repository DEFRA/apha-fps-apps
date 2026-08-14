using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Copies every year-scoped fps schema table classified <see cref="YearEndTableRuleAction.CopyToTargetYear"/>
/// in the <see cref="YearEndTableRuleMatrix"/> from the current FPS year into the target FPS year,
/// using strict year scoping. Matrix-driven and dependency-ordered: processes entries in ascending
/// <see cref="YearEndTableRuleMatrixEntry.CopyOrder"/> (the topological order established by the
/// Phase 2 FK/dependency scan) so a referenced table's target-year row always exists before the
/// referencing table is copied. This step performs a pure copy only — column-level resets for the
/// tables that need them are applied later by <see cref="ProjectFinancialResetStep"/>/
/// <see cref="ConfiguredPlanningResetStep"/>, driven by the same matrix entries'
/// <see cref="YearEndTableRuleMatrixEntry.ResetPhase"/>/<see cref="YearEndTableRuleMatrixEntry.Overrides"/>.
/// </summary>
public sealed class CopyFpsYearScopedTablesStep : IYearEndDataSetupStep
{
    private readonly ILogger<CopyFpsYearScopedTablesStep> _logger;

    public CopyFpsYearScopedTablesStep(ILogger<CopyFpsYearScopedTablesStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CopyFpsYearScopedTablesStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before FPS year-scoped copy.");
        }

        var copyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CopyToTargetYear)
            .OrderBy(e => e.CopyOrder)
            .ToList();

        foreach (var entry in copyEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CopyTableAsync(entry, context, connection, transaction, cancellationToken);
        }

        return context;
    }

    private async Task CopyTableAsync(
        YearEndTableRuleMatrixEntry entry,
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var schema = entry.Schema;
        var table = entry.TableName;
        var currentFpsYear = context.CurrentFpsYear!.Value;
        var targetFpsYear = context.TargetFpsYear!.Value;

        var exists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, schema, table, cancellationToken);
        if (!exists)
        {
            _logger.LogWarning(
                "YearEnd copy skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                context.CorrelationId,
                schema,
                table);
            return;
        }

        var hasFpsYearColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "fpsyear", cancellationToken);
        if (!hasFpsYearColumn)
        {
            throw new InvalidOperationException($"Table {schema}.{table} does not contain fpsyear and cannot be copied safely.");
        }

        var sourceRows = await CountRowsByYearAsync(connection, transaction, schema, table, currentFpsYear, cancellationToken);

        var targetRows = await CountRowsByYearAsync(connection, transaction, schema, table, targetFpsYear, cancellationToken);
        if (targetRows > 0)
        {
            throw new InvalidOperationException(
                $"Table {schema}.{table} already contains {targetRows} rows for target year {targetFpsYear}. Cleanup is required before Year End copy.");
        }

        var insertColumns = await GetInsertColumnsAsync(connection, transaction, schema, table, cancellationToken);
        if (string.IsNullOrWhiteSpace(insertColumns))
        {
            throw new InvalidOperationException($"Could not resolve copyable columns for {schema}.{table}.");
        }

        var selectProjection = await GetSelectProjectionAsync(connection, transaction, schema, table, cancellationToken);
        if (string.IsNullOrWhiteSpace(selectProjection))
        {
            throw new InvalidOperationException($"Could not resolve select projection for {schema}.{table}.");
        }

        var copied = await CopyRowsAsync(
            connection,
            transaction,
            schema,
            table,
            insertColumns,
            selectProjection,
            currentFpsYear,
            targetFpsYear,
            cancellationToken);

        _logger.LogInformation(
            "YearEnd table copy completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | CopyOrder={CopyOrder} | SourceYear={SourceYear} | TargetYear={TargetYear} | SourceRows={SourceRows} | CopiedRows={CopiedRows}",
            context.CorrelationId,
            schema,
            table,
            entry.CopyOrder,
            currentFpsYear,
            targetFpsYear,
            sourceRows,
            copied);

        if (copied != sourceRows)
        {
            // A plain copy should always insert exactly as many rows as the source year had.
            // A mismatch doesn't necessarily mean corruption (e.g. a concurrent writer), but it's
            // unexpected enough to want visible evidence for whoever reviews this run.
            _logger.LogWarning(
                "YearEnd table copy row-count mismatch | CorrelationId={CorrelationId} | Table={Schema}.{Table} | SourceRows={SourceRows} | CopiedRows={CopiedRows}",
                context.CorrelationId,
                schema,
                table,
                sourceRows,
                copied);
        }
    }

    private static async Task<long> CountRowsByYearAsync(DbConnection connection, DbTransaction transaction, string schema, string tableName, int fpsYear, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT COUNT(*) FROM {schema}.{tableName} WHERE fpsyear = @fpsyear;");
        YearEndSqlHelpers.AddParameter(command, "fpsyear", fpsYear);
        return await YearEndSqlHelpers.ExecuteCountAsync(command, cancellationToken);
    }

    private static async Task<string?> GetInsertColumnsAsync(DbConnection connection, DbTransaction transaction, string schema, string tableName, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT string_agg(format('%I', c.column_name), ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = @table_schema
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';");

        YearEndSqlHelpers.AddParameter(command, "table_schema", schema);
        YearEndSqlHelpers.AddParameter(command, "table_name", tableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<string?> GetSelectProjectionAsync(DbConnection connection, DbTransaction transaction, string schema, string tableName, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT string_agg(
                CASE
                    WHEN c.column_name = 'fpsyear' THEN '@target_fpsyear AS fpsyear'
                    ELSE format('%I', c.column_name)
                END,
                ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = @table_schema
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';");

        YearEndSqlHelpers.AddParameter(command, "table_schema", schema);
        YearEndSqlHelpers.AddParameter(command, "table_name", tableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<int> CopyRowsAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string tableName,
        string insertColumns,
        string selectProjection,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            INSERT INTO {schema}.{tableName} ({insertColumns})
            SELECT {selectProjection}
            FROM {schema}.{tableName}
            WHERE fpsyear = @current_fpsyear;");

        YearEndSqlHelpers.AddParameter(command, "current_fpsyear", currentFpsYear);
        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
