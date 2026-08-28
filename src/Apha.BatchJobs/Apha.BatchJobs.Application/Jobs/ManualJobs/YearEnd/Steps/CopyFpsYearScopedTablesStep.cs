using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Copies every year-scoped fps schema table classified <see cref="YearEndTableRuleAction.CopyToTargetYear"/>
/// in the <see cref="YearEndTableRuleMatrix"/> from the current FPS year into the target FPS year,
/// using strict year scoping. Matrix-driven and dependency-ordered: processes entries in ascending
/// <see cref="YearEndTableRuleMatrixEntry.CopyOrder"/> so a referenced table's target-year row
/// always exists before the referencing table is copied. This step performs a pure copy only —
/// column-level resets are applied later by <see cref="ProjectFinancialResetStep"/>/
/// <see cref="ConfiguredPlanningResetStep"/>.
/// </summary>
public sealed class CopyFpsYearScopedTablesStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<CopyFpsYearScopedTablesStep> _logger;

    public CopyFpsYearScopedTablesStep(
        IYearEndDataSetupRepository repository,
        ILogger<CopyFpsYearScopedTablesStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CopyFpsYearScopedTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before FPS year-scoped copy.");
        }

        var copyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CopyToTargetYear)
            .OrderBy(e => e.CopyOrder)
            .ToList();

        foreach (var entry in copyEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var schema = entry.Schema;
            var table = entry.TableName;

            if (!await _repository.TableExistsAsync(schema, table, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd copy skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    schema,
                    table);
                continue;
            }

            if (!await _repository.ColumnExistsAsync(schema, table, "fpsyear", cancellationToken))
            {
                throw new InvalidOperationException($"Table {schema}.{table} does not contain fpsyear and cannot be copied safely.");
            }

            var targetRows = await _repository.CountRowsByYearAsync(schema, table, "fpsyear", context.TargetFpsYear.Value, cancellationToken);
            if (targetRows > 0)
            {
                throw new InvalidOperationException(
                    $"Table {schema}.{table} already contains {targetRows} rows for target year {context.TargetFpsYear.Value}. Cleanup is required before Year End copy.");
            }

            var copied = await _repository.CopyFpsYearScopedTableAsync(table, context.CurrentFpsYear.Value, context.TargetFpsYear.Value, cancellationToken);

            _logger.LogInformation(
                "YearEnd table copy completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | CopyOrder={CopyOrder} | SourceYear={SourceYear} | TargetYear={TargetYear} | CopiedRows={CopiedRows}",
                context.CorrelationId,
                schema,
                table,
                entry.CopyOrder,
                context.CurrentFpsYear,
                context.TargetFpsYear,
                copied);
        }
    }
}
