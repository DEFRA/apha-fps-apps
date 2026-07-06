using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Clears configured target-year rows from tables that must start empty after setup.
/// </summary>
public sealed class TargetYearEmptyTablesStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<string> CandidateTables =
    [
        "additionalcosts_log",
        "animalreq_log",
        "fpsyeartotals",
        "mo_log",
        "monthlyoutput",
        "monthlytime",
        "mt_log",
        "proj_invoice",
        "proj_subcontract",
        "project_log",
        "projectmonth",
        "projectmonthfinal",
        "recreatesummaries_log",
        "staffjob_log",
        "tblbid",
        "tblpurchase",
        "tblsurvff_fees",
        "tblsurvff_submissions",
        "tbltestreqbaseline",
        "testreq_log",
        "timecostcalcs"
    ];

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<TargetYearEmptyTablesStep> _logger;

    public TargetYearEmptyTablesStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<TargetYearEmptyTablesStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "TargetYearEmptyTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before target-year empty-table cleanup.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        var totalDeleted = 0;

        foreach (var table in CandidateTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await TableExistsAsync(connection, "fps", table, cancellationToken))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, "fps", table, cancellationToken);
            if (yearColumn is null)
            {
                _logger.LogWarning(
                    "YearEnd empty-table cleanup skipped non-year-scoped table | CorrelationId={CorrelationId} | Table=fps.{Table}",
                    context.CorrelationId,
                    table);
                continue;
            }

            var deleted = await DeleteByYearAsync(
                connection,
                schema: "fps",
                table,
                yearColumn,
                context.TargetFpsYear.Value,
                cancellationToken);

            totalDeleted += deleted;

            _logger.LogInformation(
                "YearEnd empty-table cleanup completed | CorrelationId={CorrelationId} | Table=fps.{Table} | YearColumn={YearColumn} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
                context.CorrelationId,
                table,
                yearColumn,
                context.TargetFpsYear,
                deleted);
        }

        _logger.LogInformation(
            "YearEnd target-year empty-table cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalDeleted);
    }

    private static async Task<int> DeleteByYearAsync(
        DbConnection connection,
        string schema,
        string table,
        string yearColumn,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {schema}.{table} WHERE {yearColumn} = @target_year;";
        AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string?> ResolveYearColumnAsync(
        DbConnection connection,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        if (await ColumnExistsAsync(connection, schema, table, "fpsyear", cancellationToken))
        {
            return "fpsyear";
        }

        if (await ColumnExistsAsync(connection, schema, table, "year", cancellationToken))
        {
            return "year";
        }

        return null;
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema_name
                  AND table_name = @table_name
            );";

        AddParameter(command, "schema_name", schema);
        AddParameter(command, "table_name", table);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        string schema,
        string table,
        string column,
        CancellationToken cancellationToken)
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

        AddParameter(command, "schema_name", schema);
        AddParameter(command, "table_name", table);
        AddParameter(command, "column_name", column);
        return await ExecuteBooleanAsync(command, cancellationToken);
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
