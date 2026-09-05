using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates the schema Year End Data Setup actually consumes before any step runs: fps.tblyearmaster's
/// own columns and current-year row, every matrix entry's table and fpsyear column, the two
/// configuration tables' materialization columns, and every reset override column declared in the
/// matrix. Read-only.
/// </summary>
/// <remarks>
/// Deliberately does not check partitioning. Year End's real contract is "read current-year rows,
/// write target-year rows using the schema exposed to the application" — whether PostgreSQL implements
/// that via partitions is a DBA/schema-deployment concern, not a Year End business rule. This step used
/// to also validate every year-scoped table had partition routing for the target year; that check was
/// removed 2026-09-05 as a deliberate scope reduction, not an oversight.
/// </remarks>
public sealed class ValidateYearScopedSchemaStep : IYearEndDataSetupStep
{
    /// <summary>Columns CreatePlannedYearStep's INSERT writes — matches it exactly.</summary>
    private static readonly string[] TblYearMasterRequiredColumns =
        ["fpsyear", "fpsyearcode", "yearstatus", "remarks", "active", "createdby"];

    /// <summary>Columns MaterializeStagedSettingsAsync's INSERT writes — matches it exactly.</summary>
    private static readonly string[] TblSettingsRequiredColumns =
        ["id", "setting", "notes", "fpsyear", "updated_by", "updated_at"];

    /// <summary>Columns MaterializeStagedMonthHoursAsync's INSERT writes — matches it exactly.</summary>
    private static readonly string[] TlkpMonthHoursRequiredColumns =
        ["year", "month", "fmonth", "days", "cvlhours", "vidhours", "fpsyear"];

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<ValidateYearScopedSchemaStep> _logger;

    public ValidateYearScopedSchemaStep(
        IYearEndDataSetupRepository repository,
        ILogger<ValidateYearScopedSchemaStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ValidateYearScopedSchemaStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before schema validation.");
        }

        await ValidateRequiredColumnsAsync("fps", "tblyearmaster", TblYearMasterRequiredColumns, cancellationToken);

        var currentYearExists = await _repository.YearRowExistsAsync(context.CurrentFpsYear.Value, cancellationToken);
        if (!currentYearExists)
        {
            throw new InvalidOperationException(
                $"Current year {context.CurrentFpsYear.Value} does not exist in fps.tblyearmaster. Year End cannot continue.");
        }

        await ValidateRequiredColumnsAsync("fps", "tblsettings", TblSettingsRequiredColumns, cancellationToken);
        await ValidateRequiredColumnsAsync("fps", "tlkpmonthhours", TlkpMonthHoursRequiredColumns, cancellationToken);

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ValidateEntrySchemaAsync(entry, cancellationToken);
        }

        _logger.LogInformation(
            "YearEnd schema validation succeeded | CorrelationId={CorrelationId} | CurrentFpsYear={CurrentFpsYear} | TargetFpsYear={TargetFpsYear}",
            context.CorrelationId,
            context.CurrentFpsYear,
            context.TargetFpsYear);
    }

    /// <summary>
    /// One matrix entry's schema contract, checked uniformly for all 43 entries with no exceptions:
    /// the table and its fpsyear column must exist (needed by the generic copy mechanism, every
    /// dedicated step, and FinalValidationStep's row-count checks alike), plus every column a declared
    /// reset override targets.
    /// </summary>
    private async Task ValidateEntrySchemaAsync(YearEndTableRuleMatrixEntry entry, CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {entry.Schema}.{entry.TableName} was not found. Year End cannot continue.");
        }

        if (!await _repository.ColumnExistsAsync(entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required column {entry.Schema}.{entry.TableName}.fpsyear was not found. Year End cannot continue.");
        }

        if (entry.Overrides is null)
        {
            return;
        }

        foreach (var columnName in entry.Overrides.Keys)
        {
            if (!await _repository.ColumnExistsAsync(entry.Schema, entry.TableName, columnName, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Required reset column {entry.Schema}.{entry.TableName}.{columnName} (declared in the matrix's {entry.ResetPhase} overrides) was not found. Year End cannot continue.");
            }
        }
    }

    private async Task ValidateRequiredColumnsAsync(string schema, string table, IReadOnlyList<string> requiredColumns, CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(schema, table, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {schema}.{table} was not found. Year End cannot continue.");
        }

        foreach (var columnName in requiredColumns)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.ColumnExistsAsync(schema, table, columnName, cancellationToken))
            {
                throw new InvalidOperationException($"Required column {schema}.{table}.{columnName} was not found. Year End cannot continue.");
            }
        }
    }
}
