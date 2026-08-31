using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates that every matrix entry marked <see cref="YearEndTableRuleAction.TargetYearMustBeEmpty"/>
/// has zero target-year rows. Matrix-driven, no local table list — also re-checked independently at
/// the end of the pipeline by <see cref="FinalValidationStep"/>.
/// </summary>
/// <remarks>
/// No mutation — this only asserts absence and fails loudly if rows exist. A missing table or year
/// column is a hard failure here (not a skip): <see cref="ValidateYearScopedSchemaStep"/> already
/// guarantees this schema exists earlier in the pipeline.
/// </remarks>
public sealed class ValidateTargetYearEmptyTablesStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<ValidateTargetYearEmptyTablesStep> _logger;

    public ValidateTargetYearEmptyTablesStep(
        IYearEndDataSetupRepository repository,
        ILogger<ValidateTargetYearEmptyTablesStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ValidateTargetYearEmptyTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before target-year empty-table validation.");
        }

        var targetFpsYear = context.TargetFpsYear.Value;

        var mustBeEmptyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.TargetYearMustBeEmpty)
            .ToList();

        foreach (var entry in mustBeEmptyEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ValidateEntryAsync(entry, targetFpsYear, cancellationToken);
        }

        _logger.LogInformation(
            "YearEnd target-year empty-table validation completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | TablesChecked={TablesChecked}",
            context.CorrelationId,
            targetFpsYear,
            mustBeEmptyEntries.Count);
    }

    /// <summary>
    /// Validates one entry: table/year column must exist, target year must have zero rows. Internal
    /// (not private) so tests can exercise it directly with a synthetic entry.
    /// </summary>
    internal async Task ValidateEntryAsync(YearEndTableRuleMatrixEntry entry, int targetFpsYear, CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            throw new InvalidOperationException(
                $"Required table {entry.Schema}.{entry.TableName} does not exist — expected to already be guaranteed by ValidateYearScopedSchemaStep.");
        }

        var yearColumn = await _repository.ResolveYearColumnAsync(entry.Schema, entry.TableName, cancellationToken);
        if (yearColumn is null)
        {
            throw new InvalidOperationException(
                $"Table {entry.Schema}.{entry.TableName} does not have a resolvable year column (fpsyear/year) — expected to already be guaranteed by ValidateYearScopedSchemaStep.");
        }

        await YearEndTargetYearEmptyPolicy.EnsureTargetYearIsEmptyAsync(_repository, entry, yearColumn, targetFpsYear, cancellationToken);
    }
}
