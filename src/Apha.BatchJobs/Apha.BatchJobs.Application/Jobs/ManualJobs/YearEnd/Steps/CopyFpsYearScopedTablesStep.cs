using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Copies every matrix table marked <see cref="YearEndPrimaryRole.CopyToTargetYear"/> with no
/// <see cref="YearEndTableRuleMatrixEntry.DedicatedStep"/> from the current FPS year to the target
/// year, in ascending <see cref="YearEndTableRuleMatrixEntry.CopyOrder"/> so referenced rows exist
/// before referencing ones. Pure copy only — column resets happen later in
/// <see cref="ProjectFinancialResetStep"/>/<see cref="ConfiguredPlanningResetStep"/>. Entries with a
/// <see cref="YearEndTableRuleMatrixEntry.DedicatedStep"/> set (e.g. <c>tblperiod</c>) are handled by
/// that step instead, not this generic mechanism.
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
            .Where(e => e.PrimaryRole == YearEndPrimaryRole.CopyToTargetYear && e.DedicatedStep is null)
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
