using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Removes target-year <c>fps.tblwgemployee</c> rows (and their dependent <c>fps.tblstaffjob</c>
/// rows) for employees who were inactive in the source year and are not the General Staff
/// exemption, per the legacy <c>Annual_WGEmployeeList.sql</c> Year End rule. Replaces the earlier
/// generic active/isactive/employmentstatus/status column-discovery mechanism (which never matched
/// any real column and was consequently a no-op) with this explicit, deterministic rule, confirmed
/// 2026-08-14:
///
/// <list type="bullet">
/// <item>Inactive candidate: <c>personstatus = 'I'</c> (case-insensitive) AND
/// <c>enddate IS NULL</c>, evaluated against the <b>target</b> year's own row only.</item>
/// <item>General Staff exemption (retained even if inactive):
/// <c>spnumber LIKE 'G%'</c> (case-sensitive, matching the legacy rule) AND
/// <c>UPPER(firstname) = 'GENERAL'</c>. Both conditions required — confirmed via a read-only
/// cross-tab against live data that this AND reading is not equivalent to OR (15 discordant rows
/// exist); the AND reading is the one confirmed by the legacy script.</item>
/// <item>Any <c>personstatus</c> value other than <c>A</c>/<c>a</c>/<c>I</c>/<c>i</c> is a
/// data-quality error, surfaced before any deletion — never silently treated as active or
/// inactive.</item>
/// </list>
///
/// FPS-only: no <c>mabarchive</c> table is referenced. <c>mabarchive.my_tblwgemployee</c> does not
/// even exist in <c>batchjob_testing</c>, and MABArchive participation in Year End is gated
/// exclusively through <see cref="ConditionalMabArchiveYearSetupStep"/> — this step must not reach
/// into MABArchive outside that gate.
/// </summary>
public sealed class InactiveEmployeeCleanupStep : IYearEndDataSetupStep
{
    private const string Schema = "fps";

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

        var targetFpsYear = context.TargetFpsYear.Value;

        await ValidatePersonStatusValuesAsync(connection, transaction, targetFpsYear, cancellationToken);

        var eligiblePactIds = await GetInactiveNonGeneralStaffPactIdsAsync(connection, transaction, targetFpsYear, cancellationToken);
        if (eligiblePactIds.Count == 0)
        {
            _logger.LogInformation(
                "YearEnd inactive employee cleanup found no applicable rows | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
                context.CorrelationId,
                targetFpsYear);
            return context;
        }

        // tblstaffjob has an FK to tblwgemployee — dependent rows must go first.
        var staffJobDeleted = await DeleteByKeyValuesAsync(
            connection, transaction, Schema, "tblstaffjob", "staffid", "fpsyear", targetFpsYear, eligiblePactIds, cancellationToken);

        var wgEmployeeDeleted = await DeleteByKeyValuesAsync(
            connection, transaction, Schema, "tblwgemployee", "pactid", "fpsyear", targetFpsYear, eligiblePactIds, cancellationToken);

        _logger.LogInformation(
            "YearEnd inactive employee cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | " +
            "InactiveNonGeneralStaffCount={EligibleCount} | TblStaffJobRowsDeleted={StaffJobDeleted} | TblWgEmployeeRowsDeleted={WgEmployeeDeleted}",
            context.CorrelationId,
            targetFpsYear,
            eligiblePactIds.Count,
            staffJobDeleted,
            wgEmployeeDeleted);

        return context;
    }

    /// <summary>
    /// Data-quality gate, run before any deletion: every target-year <c>personstatus</c> value must
    /// be <c>A</c>/<c>a</c>/<c>I</c>/<c>i</c>. Anything else (e.g. the known <c>AI</c> value found in
    /// live data) is ambiguous and must block cleanup entirely rather than be silently classified
    /// either way.
    /// </summary>
    private static async Task ValidatePersonStatusValuesAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            SELECT pactid, personstatus
            FROM {Schema}.tblwgemployee
            WHERE fpsyear = @target_fpsyear
              AND UPPER(personstatus) NOT IN ('A', 'I')
            ORDER BY pactid
            LIMIT 20;");

        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);

        var unexpected = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var pactid = reader.IsDBNull(0) ? "<null>" : reader.GetString(0);
                var personStatus = reader.IsDBNull(1) ? "<null>" : reader.GetString(1);
                unexpected.Add($"{pactid}='{personStatus}'");
            }
        }

        if (unexpected.Count > 0)
        {
            throw new InvalidOperationException(
                $"Target year {targetFpsYear} has {Schema}.tblwgemployee rows with an unexpected personstatus value " +
                $"(expected only A/a/I/i): {string.Join(", ", unexpected)}. Resolve the data quality issue before Year End cleanup can proceed.");
        }
    }

    /// <summary>
    /// Target-year <c>pactid</c> values for employees who are inactive
    /// (<c>personstatus='I'</c> case-insensitive, <c>enddate IS NULL</c>) and not the General Staff
    /// exemption (<c>spnumber LIKE 'G%' AND UPPER(firstname)='GENERAL'</c>, both required). The join
    /// to <c>tblemployee</c> is year-scoped and inner — an employee with no matching target-year
    /// <c>tblemployee</c> row (so their name, and therefore General Staff status, can't be
    /// determined) is never treated as eligible for removal.
    /// </summary>
    private static async Task<IReadOnlyList<string>> GetInactiveNonGeneralStaffPactIdsAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            SELECT wg.pactid
            FROM {Schema}.tblwgemployee wg
            JOIN {Schema}.tblemployee e
              ON e.spnumber = wg.spnumber
             AND e.fpsyear = wg.fpsyear
            WHERE wg.fpsyear = @target_fpsyear
              AND UPPER(wg.personstatus) = 'I'
              AND wg.enddate IS NULL
              AND NOT (wg.spnumber LIKE 'G%' AND UPPER(TRIM(e.firstname)) = 'GENERAL');");

        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);

        var pactIds = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pactIds.Add(reader.GetString(0));
        }

        return pactIds;
    }

    private static async Task<int> DeleteByKeyValuesAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string keyColumn,
        string yearColumn,
        int targetFpsYear,
        IReadOnlyList<string> keyValues,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            DELETE FROM {schema}.{table}
            WHERE {yearColumn} = @target_fpsyear
              AND {keyColumn} = ANY(@key_values);");

        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);
        YearEndSqlHelpers.AddParameter(command, "key_values", keyValues.ToArray());

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
