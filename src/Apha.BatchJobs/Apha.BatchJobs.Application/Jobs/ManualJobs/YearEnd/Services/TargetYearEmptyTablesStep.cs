using System.Data.Common;
using Microsoft.Extensions.Logging;

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

    private readonly ILogger<TargetYearEmptyTablesStep> _logger;

    public TargetYearEmptyTablesStep(ILogger<TargetYearEmptyTablesStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "TargetYearEmptyTablesStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before target-year empty-table cleanup.");
        }

        var totalDeleted = 0;

        foreach (var table in CandidateTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, "fps", table, cancellationToken))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, transaction, "fps", table, cancellationToken);
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
                transaction,
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

        return context;
    }

    private static async Task<int> DeleteByYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string yearColumn,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"DELETE FROM {schema}.{table} WHERE {yearColumn} = @target_year;");
        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
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
