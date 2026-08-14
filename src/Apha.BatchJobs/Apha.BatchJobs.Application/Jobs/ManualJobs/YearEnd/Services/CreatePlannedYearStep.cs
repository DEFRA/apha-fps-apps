using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Creates the target FPS year in fps.tblyearmaster as Planned when it does not exist.
/// </summary>
public sealed class CreatePlannedYearStep : IYearEndDataSetupStep
{
    private const string PlannedStatus = "Planned";
    private const string BatchCreatedBy = "YearEndBatchWorker";

    private readonly ILogger<CreatePlannedYearStep> _logger;

    public CreatePlannedYearStep(ILogger<CreatePlannedYearStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CreatePlannedYearStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Data Setup requires targetFpsYear before creating planned year.");
        }

        var targetYear = context.TargetFpsYear.Value;
        var targetYearState = await GetYearStateAsync(connection, transaction, targetYear, cancellationToken);

        if (targetYearState is not null)
        {
            if (!string.Equals(targetYearState.YearStatus, PlannedStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Target year {targetYear} already exists in fps.tblyearmaster with status '{targetYearState.YearStatus}'. Expected '{PlannedStatus}'.");
            }

            if (!targetYearState.Active)
            {
                throw new InvalidOperationException(
                    $"Target year {targetYear} exists in fps.tblyearmaster but is inactive. Manual review is required.");
            }

            _logger.LogInformation(
                "YearEnd target year already exists as Planned | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear}",
                context.CorrelationId,
                targetYear);
            return context;
        }

        var targetYearCode = BuildFpsYearCode(targetYear);
        var insertCount = await InsertPlannedYearAsync(
            connection,
            transaction,
            targetYear,
            targetYearCode,
            context.CorrelationId,
            cancellationToken);

        if (insertCount != 1)
        {
            throw new InvalidOperationException(
                $"Expected to insert one row for target year {targetYear}, but inserted {insertCount} rows.");
        }

        _logger.LogInformation(
            "YearEnd planned year created | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | FpsYearCode={FpsYearCode}",
            context.CorrelationId,
            targetYear,
            targetYearCode);

        return context;
    }

    private static string BuildFpsYearCode(int fpsYear)
    {
        var followingYearTwoDigits = (fpsYear + 1) % 100;
        return $"FPS{fpsYear}-{followingYearTwoDigits:D2}";
    }

    private static async Task<YearState?> GetYearStateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @fpsyear;");

        YearEndSqlHelpers.AddParameter(command, "fpsyear", fpsYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        return new YearState(yearStatus, active);
    }

    private static async Task<int> InsertPlannedYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        string fpsYearCode,
        string correlationId,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES (@fpsyear, @fpsyearcode, @yearstatus, @remarks, @active, @createdby);");

        YearEndSqlHelpers.AddParameter(command, "fpsyear", fpsYear);
        YearEndSqlHelpers.AddParameter(command, "fpsyearcode", fpsYearCode);
        YearEndSqlHelpers.AddParameter(command, "yearstatus", PlannedStatus);
        YearEndSqlHelpers.AddParameter(command, "remarks", $"Created by YearEndDataSetup. CorrelationId={correlationId}");
        YearEndSqlHelpers.AddParameter(command, "active", true);
        YearEndSqlHelpers.AddParameter(command, "createdby", BatchCreatedBy);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record YearState(string YearStatus, bool Active);
}
