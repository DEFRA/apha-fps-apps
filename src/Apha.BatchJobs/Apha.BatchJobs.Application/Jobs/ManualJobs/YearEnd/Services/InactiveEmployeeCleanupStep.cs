using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Removes target-year staff-job rows that map to inactive employees when inactive markers are available.
/// </summary>
public sealed class InactiveEmployeeCleanupStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<CleanupTarget> Targets =
    [
        new("fps", "tblstaffjob", "fpsyear", "staffid", "tblwgemployee", "pactid"),
        new("mabarchive", "my_tblstaffjob", "year", "staffid", "my_tblwgemployee", "pactid")
    ];

    private readonly ILogger<InactiveEmployeeCleanupStep> _logger;

    public InactiveEmployeeCleanupStep(ILogger<InactiveEmployeeCleanupStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "InactiveEmployeeCleanupStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before inactive employee cleanup.");
        }

        var totalDeleted = 0;

        foreach (var target in Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deleted = await CleanupTargetAsync(connection, transaction, target, context, cancellationToken);
            totalDeleted += deleted;
        }

        _logger.LogInformation(
            "YearEnd inactive employee cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalDeleted);

        return context;
    }

    private async Task<int> CleanupTargetAsync(
        DbConnection connection,
        DbTransaction transaction,
        CleanupTarget target,
        YearEndExecutionContext context,
        CancellationToken cancellationToken)
    {
        var jobTableExists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, target.Schema, target.JobTable, cancellationToken);
        if (!jobTableExists)
        {
            _logger.LogWarning(
                "YearEnd inactive cleanup skipped missing job table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                context.CorrelationId,
                target.Schema,
                target.JobTable);
            return 0;
        }

        var employeeTableExists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, target.Schema, target.EmployeeTable, cancellationToken);
        if (!employeeTableExists)
        {
            _logger.LogWarning(
                "YearEnd inactive cleanup skipped missing employee table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                context.CorrelationId,
                target.Schema,
                target.EmployeeTable);
            return 0;
        }

        var hasYearColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, target.Schema, target.JobTable, target.YearColumn, cancellationToken);
        var hasStaffColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, target.Schema, target.JobTable, target.JobStaffColumn, cancellationToken);
        var hasEmployeeStaffColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, target.Schema, target.EmployeeTable, target.EmployeeStaffColumn, cancellationToken);

        if (!hasYearColumn || !hasStaffColumn || !hasEmployeeStaffColumn)
        {
            throw new InvalidOperationException(
                $"Inactive cleanup cannot run safely for {target.Schema}.{target.JobTable}; required columns are missing.");
        }

        var inactivePredicate = await BuildInactivePredicateAsync(connection, transaction, target.Schema, target.EmployeeTable, cancellationToken);
        if (string.IsNullOrWhiteSpace(inactivePredicate))
        {
            _logger.LogWarning(
                "YearEnd inactive cleanup skipped because no inactive markers were found | CorrelationId={CorrelationId} | EmployeeTable={Schema}.{Table}",
                context.CorrelationId,
                target.Schema,
                target.EmployeeTable);
            return 0;
        }

        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            DELETE FROM {target.Schema}.{target.JobTable} sj
            WHERE sj.{target.YearColumn} = @target_year
              AND EXISTS (
                    SELECT 1
                    FROM {target.Schema}.{target.EmployeeTable} e
                    WHERE e.{target.EmployeeStaffColumn} = sj.{target.JobStaffColumn}
                      AND ({inactivePredicate})
              );");

        YearEndSqlHelpers.AddParameter(command, "target_year", context.TargetFpsYear!.Value);
        var deleted = await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation(
            "YearEnd inactive cleanup completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            target.Schema,
            target.JobTable,
            context.TargetFpsYear,
            deleted);

        return deleted;
    }

    private static async Task<string?> BuildInactivePredicateAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        var predicates = new List<string>();

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "active", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.active as text), '')) in ('false', '0', 'n', 'no')");
        }

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "isactive", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.isactive as text), '')) in ('false', '0', 'n', 'no')");
        }

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "employmentstatus", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.employmentstatus as text), '')) in ('inactive', 'leaver', 'left', 'terminated')");
        }

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "status", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.status as text), '')) in ('inactive', 'leaver', 'left', 'terminated')");
        }

        return predicates.Count == 0 ? null : string.Join(" OR ", predicates);
    }

    private sealed record CleanupTarget(
        string Schema,
        string JobTable,
        string YearColumn,
        string JobStaffColumn,
        string EmployeeTable,
        string EmployeeStaffColumn);
}
