using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Validates final target-year setup state before Year End Data Setup completion, inside the same
/// transaction as every prior step.
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

    private readonly ILogger<FinalValidationStep> _logger;

    public FinalValidationStep(ILogger<FinalValidationStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "FinalValidationStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before final validation.");
        }

        await ValidateTargetYearMasterStateAsync(connection, transaction, context.TargetFpsYear.Value, cancellationToken);
        await ValidateRequiredTargetDataAsync(connection, transaction, context.TargetFpsYear.Value, cancellationToken);
        await ValidateTargetYearEmptyTablesAsync(connection, transaction, context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd final validation completed | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
            context.CorrelationId,
            context.TargetFpsYear);

        return context;
    }

    private static async Task ValidateTargetYearMasterStateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @target_year;");

        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);

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
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        foreach (var (schema, table, yearColumn) in RequiredTargetYearDataTables)
        {
            if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, schema, table, cancellationToken))
            {
                continue;
            }

            if (!await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, yearColumn, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Required validation table {schema}.{table} does not contain year column {yearColumn}.");
            }

            var count = await CountByYearAsync(connection, transaction, schema, table, yearColumn, targetYear, cancellationToken);
            if (count <= 0)
            {
                throw new InvalidOperationException(
                    $"Expected target-year rows in {schema}.{table} for year {targetYear}, but found none.");
            }
        }
    }

    private static async Task ValidateTargetYearEmptyTablesAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        foreach (var table in MustBeEmptyTargetYearTables)
        {
            if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, "fps", table, cancellationToken))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, transaction, "fps", table, cancellationToken);
            if (yearColumn is null)
            {
                continue;
            }

            var count = await CountByYearAsync(connection, transaction, "fps", table, yearColumn, targetYear, cancellationToken);
            if (count != 0)
            {
                throw new InvalidOperationException(
                    $"Expected no target-year rows in fps.{table} for year {targetYear}, but found {count}.");
            }
        }
    }

    private static async Task<long> CountByYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string yearColumn,
        int year,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @target_year;");
        YearEndSqlHelpers.AddParameter(command, "target_year", year);
        return await YearEndSqlHelpers.ExecuteCountAsync(command, cancellationToken);
    }

    private static async Task<string?> ResolveYearColumnAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "fpsyear", cancellationToken))
        {
            return "fpsyear";
        }

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "year", cancellationToken))
        {
            return "year";
        }

        return null;
    }
}
