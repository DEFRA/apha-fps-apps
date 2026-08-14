using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Copies fps.tblperiod rows from current FPS year into target FPS year with strict year scoping.
/// </summary>
public sealed class PeriodSetupStep : IYearEndDataSetupStep
{
    private const string TableSchema = "fps";
    private const string TableName = "tblperiod";
    private const string YearColumn = "fpsyear";
    private const string PeriodLockColumn = "periodlocked";

    private readonly ILogger<PeriodSetupStep> _logger;

    public PeriodSetupStep(ILogger<PeriodSetupStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "PeriodSetupStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before period setup.");
        }

        if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, TableSchema, TableName, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {TableSchema}.{TableName} was not found.");
        }

        if (!await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, TableSchema, TableName, YearColumn, cancellationToken))
        {
            throw new InvalidOperationException($"Required column {TableSchema}.{TableName}.{YearColumn} was not found.");
        }

        var currentCount = await CountRowsByYearAsync(connection, transaction, context.CurrentFpsYear.Value, cancellationToken);
        if (currentCount == 0)
        {
            throw new InvalidOperationException(
                $"Source year {context.CurrentFpsYear.Value} has no rows in {TableSchema}.{TableName}; cannot prepare target period rows.");
        }

        var targetCount = await CountRowsByYearAsync(connection, transaction, context.TargetFpsYear.Value, cancellationToken);
        if (targetCount > 0)
        {
            throw new InvalidOperationException(
                $"Target year {context.TargetFpsYear.Value} already has {targetCount} rows in {TableSchema}.{TableName}. Cleanup is required before period setup.");
        }

        var insertColumns = await GetInsertColumnsAsync(connection, transaction, cancellationToken);
        if (string.IsNullOrWhiteSpace(insertColumns))
        {
            throw new InvalidOperationException($"Could not resolve copyable columns for {TableSchema}.{TableName}.");
        }

        var hasPeriodLockedColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, TableSchema, TableName, PeriodLockColumn, cancellationToken);
        var selectProjection = await GetSelectProjectionAsync(connection, transaction, hasPeriodLockedColumn, cancellationToken);
        if (string.IsNullOrWhiteSpace(selectProjection))
        {
            throw new InvalidOperationException($"Could not resolve select projection for {TableSchema}.{TableName}.");
        }

        var inserted = await CopyRowsAsync(
            connection,
            transaction,
            insertColumns,
            selectProjection,
            context.CurrentFpsYear.Value,
            context.TargetFpsYear.Value,
            cancellationToken);

        _logger.LogInformation(
            "YearEnd period setup completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | SourceYear={SourceYear} | TargetYear={TargetYear} | InsertedRows={InsertedRows} | PeriodLockReset={PeriodLockReset}",
            context.CorrelationId,
            TableSchema,
            TableName,
            context.CurrentFpsYear,
            context.TargetFpsYear,
            inserted,
            hasPeriodLockedColumn);

        return context;
    }

    private static async Task<long> CountRowsByYearAsync(DbConnection connection, DbTransaction transaction, int fpsYear, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT COUNT(*) FROM {TableSchema}.{TableName} WHERE {YearColumn} = @fpsyear;");
        YearEndSqlHelpers.AddParameter(command, "fpsyear", fpsYear);
        return await YearEndSqlHelpers.ExecuteCountAsync(command, cancellationToken);
    }

    private static async Task<string?> GetInsertColumnsAsync(DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT string_agg(format('%I', c.column_name), ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = @schema_name
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';");

        YearEndSqlHelpers.AddParameter(command, "schema_name", TableSchema);
        YearEndSqlHelpers.AddParameter(command, "table_name", TableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<string?> GetSelectProjectionAsync(
        DbConnection connection,
        DbTransaction transaction,
        bool hasPeriodLockedColumn,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT string_agg(
                CASE
                    WHEN c.column_name = @year_column THEN '@target_fpsyear AS ' || format('%I', c.column_name)
                    WHEN c.column_name = @period_lock_column AND @reset_period_lock THEN '0 AS ' || format('%I', c.column_name)
                    ELSE format('%I', c.column_name)
                END,
                ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = @schema_name
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';");

        YearEndSqlHelpers.AddParameter(command, "schema_name", TableSchema);
        YearEndSqlHelpers.AddParameter(command, "table_name", TableName);
        YearEndSqlHelpers.AddParameter(command, "year_column", YearColumn);
        YearEndSqlHelpers.AddParameter(command, "period_lock_column", PeriodLockColumn);
        YearEndSqlHelpers.AddParameter(command, "reset_period_lock", hasPeriodLockedColumn);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<int> CopyRowsAsync(
        DbConnection connection,
        DbTransaction transaction,
        string insertColumns,
        string selectProjection,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            INSERT INTO {TableSchema}.{TableName} ({insertColumns})
            SELECT {selectProjection}
            FROM {TableSchema}.{TableName}
            WHERE {YearColumn} = @current_fpsyear;");

        YearEndSqlHelpers.AddParameter(command, "current_fpsyear", currentFpsYear);
        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
