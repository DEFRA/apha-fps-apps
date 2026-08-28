using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates that every <see cref="YearEndTableRuleMatrix"/> entry classified
/// <see cref="YearEndTableRuleAction.TargetYearMustBeEmpty"/> (the spec §19 "must start empty in the
/// target year" legacy candidates) has zero target-year rows. Matrix-driven — no local table list; the
/// matrix is the single authoritative list, also consumed by <see cref="FinalValidationStep"/> to
/// independently re-verify the same result at the end of the pipeline.
/// </summary>
/// <remarks>
/// Renamed and rewritten 2026-08-28 (Phase 7B) off the Year End Process New Approach workbook: every
/// one of these 21 tables' old-architecture <c>Annual_UpdateOtherTables.sql</c> DELETE is marked N/A
/// there, remarked "Year Identification column in table". The one-database-per-year architecture
/// deleted these tables' contents as part of Year End; the current multi-year, single-database
/// architecture never copies/inserts target-year rows for them at all (see
/// <see cref="CopyFpsYearScopedTablesStep"/>, which filters strictly to
/// <see cref="YearEndTableRuleAction.CopyToTargetYear"/>), so the correct production behaviour is to
/// assert absence and fail loudly if it doesn't hold — never a DELETE. This step performs no mutation.
///
/// <para>
/// A missing table or unresolvable year column is a hard failure here, not a skip:
/// <see cref="ValidateYearScopedSchemaStep"/> runs earlier specifically to guarantee these 21 tables'
/// schema exists, and silently tolerating its absence this late would quietly weaken that guarantee
/// and could mask pipeline/configuration drift. <see cref="FinalValidationStep"/>'s independent
/// re-check keeps its own pre-existing skip-if-missing behaviour — it is defense-in-depth, not the
/// schema's contract owner.
/// </para>
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
    /// Validates one matrix entry: table and year column must exist (a hard failure otherwise — see
    /// the type-level remarks), and the target year must have zero rows in it. Internal, rather than
    /// private, purely so tests can exercise this step's missing-table/missing-year-column failure
    /// modes directly with a synthetic <see cref="YearEndTableRuleMatrixEntry"/>, without needing to
    /// fake an entry into the real, single-source-of-truth <see cref="YearEndTableRuleMatrix"/>.
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
