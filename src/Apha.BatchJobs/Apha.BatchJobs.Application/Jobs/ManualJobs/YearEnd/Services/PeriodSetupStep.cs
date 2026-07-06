using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

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

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<PeriodSetupStep> _logger;

    public PeriodSetupStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<PeriodSetupStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "PeriodSetupStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before period setup.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        if (!await TableExistsAsync(connection, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {TableSchema}.{TableName} was not found.");
        }

        if (!await ColumnExistsAsync(connection, YearColumn, cancellationToken))
        {
            throw new InvalidOperationException($"Required column {TableSchema}.{TableName}.{YearColumn} was not found.");
        }

        var currentCount = await CountRowsByYearAsync(connection, context.CurrentFpsYear.Value, cancellationToken);
        if (currentCount == 0)
        {
            throw new InvalidOperationException(
                $"Source year {context.CurrentFpsYear.Value} has no rows in {TableSchema}.{TableName}; cannot prepare target period rows.");
        }

        var targetCount = await CountRowsByYearAsync(connection, context.TargetFpsYear.Value, cancellationToken);
        if (targetCount > 0)
        {
            throw new InvalidOperationException(
                $"Target year {context.TargetFpsYear.Value} already has {targetCount} rows in {TableSchema}.{TableName}. Cleanup is required before period setup.");
        }

        var insertColumns = await GetInsertColumnsAsync(connection, cancellationToken);
        if (string.IsNullOrWhiteSpace(insertColumns))
        {
            throw new InvalidOperationException($"Could not resolve copyable columns for {TableSchema}.{TableName}.");
        }

        var hasPeriodLockedColumn = await ColumnExistsAsync(connection, PeriodLockColumn, cancellationToken);
        var selectProjection = await GetSelectProjectionAsync(connection, hasPeriodLockedColumn, cancellationToken);
        if (string.IsNullOrWhiteSpace(selectProjection))
        {
            throw new InvalidOperationException($"Could not resolve select projection for {TableSchema}.{TableName}.");
        }

        var inserted = await CopyRowsAsync(
            connection,
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
    }

    private static async Task<bool> TableExistsAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema_name
                  AND table_name = @table_name
            );";

        AddParameter(command, "schema_name", TableSchema);
        AddParameter(command, "table_name", TableName);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection connection, string columnName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = @schema_name
                  AND table_name = @table_name
                  AND column_name = @column_name
            );";

        AddParameter(command, "schema_name", TableSchema);
        AddParameter(command, "table_name", TableName);
        AddParameter(command, "column_name", columnName);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<long> CountRowsByYearAsync(DbConnection connection, int fpsYear, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {TableSchema}.{TableName} WHERE {YearColumn} = @fpsyear;";
        AddParameter(command, "fpsyear", fpsYear);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is long count ? count : Convert.ToInt64(scalar);
    }

    private static async Task<string?> GetInsertColumnsAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT string_agg(format('%I', c.column_name), ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = @schema_name
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';";

        AddParameter(command, "schema_name", TableSchema);
        AddParameter(command, "table_name", TableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<string?> GetSelectProjectionAsync(
        DbConnection connection,
        bool hasPeriodLockedColumn,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
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
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';";

        AddParameter(command, "schema_name", TableSchema);
        AddParameter(command, "table_name", TableName);
        AddParameter(command, "year_column", YearColumn);
        AddParameter(command, "period_lock_column", PeriodLockColumn);
        AddParameter(command, "reset_period_lock", hasPeriodLockedColumn);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<int> CopyRowsAsync(
        DbConnection connection,
        string insertColumns,
        string selectProjection,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            INSERT INTO {TableSchema}.{TableName} ({insertColumns})
            SELECT {selectProjection}
            FROM {TableSchema}.{TableName}
            WHERE {YearColumn} = @current_fpsyear;";

        AddParameter(command, "current_fpsyear", currentFpsYear);
        AddParameter(command, "target_fpsyear", targetFpsYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> ExecuteBooleanAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool value && value;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}