using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Live re-validation of target-year configuration inside the Data Setup transaction. Mirrors the
/// rules already enforced at initiation time by <c>Apha.FPS.Application.Services.YearEndService.
/// ValidateConfiguration</c> (against the same <c>fps.tblsettings</c>/<c>fps.tlkpmonthhours</c>
/// tables) — a live re-check matters because time can pass between approval and execution. Not
/// <c>configuration_json</c>, which does not exist in the Year End contract.
/// </summary>
public sealed class ValidateYearEndConfigurationStep : IYearEndDataSetupStep
{
    private const string HoursInDaySettingId = "hoursinday";
    private const string CapApprovalReceivedForResetSettingId = "capapprovalreceivedforreset";

    private readonly ILogger<ValidateYearEndConfigurationStep> _logger;

    public ValidateYearEndConfigurationStep(ILogger<ValidateYearEndConfigurationStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ValidateYearEndConfigurationStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before configuration validation.");
        }

        var targetYear = context.TargetFpsYear.Value;

        await ValidateHoursInDayAsync(connection, transaction, targetYear, cancellationToken);
        await ValidateCapApprovalReceivedForResetAsync(connection, transaction, targetYear, cancellationToken);
        await ValidateMonthHoursAsync(connection, transaction, targetYear, cancellationToken);

        _logger.LogInformation(
            "YearEnd configuration validation succeeded | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
            context.CorrelationId,
            targetYear);

        return context;
    }

    private static async Task ValidateHoursInDayAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        var value = await GetSettingValueAsync(connection, transaction, targetYear, HoursInDaySettingId, cancellationToken);

        if (value is null || !decimal.TryParse(value, out var hoursInDay) || hoursInDay <= 0)
        {
            throw new InvalidOperationException(
                $"Configuration value 'HoursInDay' is missing or not a positive number for target year {targetYear}.");
        }
    }

    private static async Task ValidateCapApprovalReceivedForResetAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        var value = await GetSettingValueAsync(connection, transaction, targetYear, CapApprovalReceivedForResetSettingId, cancellationToken);
        var normalized = value?.Trim();

        var isValid = string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            throw new InvalidOperationException(
                $"Configuration value 'CapApprovalReceivedForReset' must be 'Yes' or 'No' for target year {targetYear}.");
        }
    }

    private static async Task<string?> GetSettingValueAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        string settingId,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT setting
            FROM fps.tblsettings
            WHERE fpsyear = @target_year
              AND lower(id) = @setting_id;");

        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);
        YearEndSqlHelpers.AddParameter(command, "setting_id", settingId);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar as string;
    }

    private static async Task ValidateMonthHoursAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT month, days, vidhours, cvlhours
            FROM fps.tlkpmonthhours
            WHERE fpsyear = @target_year;");

        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);

        var monthsFound = new HashSet<short>();
        var hasNegativeValue = false;

        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var month = reader.GetInt16(0);
                monthsFound.Add(month);

                if (reader.IsDBNull(1) || reader.IsDBNull(2) || reader.IsDBNull(3))
                {
                    throw new InvalidOperationException(
                        $"Month {month} configuration for target year {targetYear} is missing Days/VidHours/CvlHours.");
                }

                if (reader.GetDecimal(1) < 0 || reader.GetDecimal(2) < 0 || reader.GetDecimal(3) < 0)
                {
                    hasNegativeValue = true;
                }
            }
        }

        var missingMonths = Enumerable.Range(1, 12).Where(month => !monthsFound.Contains((short)month)).ToList();
        if (missingMonths.Count > 0)
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} is missing month-hours configuration for month(s): {string.Join(", ", missingMonths)}.");
        }

        if (hasNegativeValue)
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} has negative Days/VidHours/CvlHours in fps.tlkpmonthhours.");
        }
    }
}
