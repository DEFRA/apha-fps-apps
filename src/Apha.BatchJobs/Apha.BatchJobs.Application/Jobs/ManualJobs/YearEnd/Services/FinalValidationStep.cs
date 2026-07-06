using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Validates final target-year setup state before Year End Data Setup completion.
/// </summary>
public sealed class FinalValidationStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<(string Schema, string Table, string YearColumn)> RequiredTargetYearDataTables =
    [
        ("fps", "tlkpproject", "fpsyear"),
        ("fps", "tblstaffjob", "fpsyear"),
        ("fps", "tlkptestreqmt", "fpsyear"),
        ("fps", "tblanimalreq", "fpsyear"),
        ("fps", "tbladditionalcosts", "fpsyear"),
        ("fps", "tblperiod", "fpsyear"),
        ("mabarchive", "my_tlkpproject", "year"),
        ("mabarchive", "my_tblstaffjob", "year"),
        ("mabarchive", "my_tlkptestreqmt", "year"),
        ("mabarchive", "my_tblanimalreq", "year"),
        ("mabarchive", "my_tbladditionalcosts", "year")
    ];

    private static readonly IReadOnlyList<string> MustBeEmptyTargetYearTables =
    [
        "monthlyoutput",
        "monthlytime",
        "proj_invoice",
        "proj_subcontract",
        "projectmonth",
        "projectmonthfinal",
        "timecostcalcs"
    ];

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<FinalValidationStep> _logger;

    public FinalValidationStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<FinalValidationStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "FinalValidationStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before final validation.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await ValidateTargetYearMasterStateAsync(connection, context.TargetFpsYear.Value, cancellationToken);
        await ValidateRequiredTargetDataAsync(connection, context.TargetFpsYear.Value, cancellationToken);
        await ValidateTargetYearEmptyTablesAsync(connection, context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd final validation completed | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
            context.CorrelationId,
            context.TargetFpsYear);
    }

    private static async Task ValidateTargetYearMasterStateAsync(
        DbConnection connection,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @target_year;";

        AddParameter(command, "target_year", targetYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Target year {targetYear} does not exist in fps.tblyearmaster.");
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        if (!string.Equals(yearStatus, "Planned", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} is in status '{yearStatus}', expected 'Planned' before cutover.");
        }

        if (!active)
        {
            throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
        }
    }

    private static async Task ValidateRequiredTargetDataAsync(
        DbConnection connection,
        int targetYear,
        CancellationToken cancellationToken)
    {
        foreach (var (schema, table, yearColumn) in RequiredTargetYearDataTables)
        {
            if (!await TableExistsAsync(connection, schema, table, cancellationToken))
            {
                continue;
            }

            if (!await ColumnExistsAsync(connection, schema, table, yearColumn, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Required validation table {schema}.{table} does not contain year column {yearColumn}.");
            }

            var count = await CountByYearAsync(connection, schema, table, yearColumn, targetYear, cancellationToken);
            if (count <= 0)
            {
                throw new InvalidOperationException(
                    $"Expected target-year rows in {schema}.{table} for year {targetYear}, but found none.");
            }
        }
    }

    private static async Task ValidateTargetYearEmptyTablesAsync(
        DbConnection connection,
        int targetYear,
        CancellationToken cancellationToken)
    {
        foreach (var table in MustBeEmptyTargetYearTables)
        {
            if (!await TableExistsAsync(connection, "fps", table, cancellationToken))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, "fps", table, cancellationToken);
            if (yearColumn is null)
            {
                continue;
            }

            var count = await CountByYearAsync(connection, "fps", table, yearColumn, targetYear, cancellationToken);
            if (count != 0)
            {
                throw new InvalidOperationException(
                    $"Expected no target-year rows in fps.{table} for year {targetYear}, but found {count}.");
            }
        }
    }

    private static async Task<long> CountByYearAsync(
        DbConnection connection,
        string schema,
        string table,
        string yearColumn,
        int year,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @target_year;";
        AddParameter(command, "target_year", year);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is long count ? count : Convert.ToInt64(scalar);
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
