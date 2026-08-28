using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates final target-year setup state before Year End Data Setup completion. Matrix-driven —
/// dispatches validation per <see cref="YearEndTableRuleMatrix"/> entry by
/// <see cref="YearEndTableRuleMatrixEntry.Action"/>, not a second hardcoded table list.
/// </summary>
public sealed class FinalValidationStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<FinalValidationStep> _logger;

    public FinalValidationStep(
        IYearEndDataSetupRepository repository,
        ILogger<FinalValidationStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "FinalValidationStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before final validation.");
        }

        var currentFpsYear = context.CurrentFpsYear.Value;
        var targetFpsYear = context.TargetFpsYear.Value;

        await ValidateTargetYearMasterStateAsync(targetFpsYear, cancellationToken);

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ValidateEntryAsync(entry, currentFpsYear, targetFpsYear, cancellationToken);
        }

        _logger.LogInformation(
            "YearEnd final validation completed | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
            context.CorrelationId,
            targetFpsYear);
    }

    private async Task ValidateEntryAsync(
        YearEndTableRuleMatrixEntry entry,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        switch (entry.Action)
        {
            case YearEndTableRuleAction.CopyToTargetYear:
                await ValidateCopiedTableAsync(entry, currentFpsYear, targetFpsYear, cancellationToken);
                break;

            case YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep:
                // tblperiod today. Only proves the dedicated step actually produced rows.
                await ValidateDedicatedStepTableHasTargetRowsAsync(entry, targetFpsYear, cancellationToken);
                break;

            case YearEndTableRuleAction.TargetYearMustBeEmpty:
                await ValidateTargetYearIsEmptyAsync(entry, targetFpsYear, cancellationToken);
                break;

            case YearEndTableRuleAction.ValidateExists:
                await ValidateExistsAsync(entry, targetFpsYear, cancellationToken);
                break;

            default:
                // PendingClassification/CreateTargetYearRow/ResetTargetYearRows/SkipLegacyObsolete/
                // ManualReviewRequired: none of these are ever expected on a live matrix entry.
                // Deliberately no default validation — an unresolved action reaching here is a
                // matrix authoring gap, not something to silently pass or silently skip.
                throw new InvalidOperationException(
                    $"Matrix entry {entry.Schema}.{entry.TableName} has action {entry.Action}, which final validation does not know how to check.");
        }
    }

    private async Task ValidateTargetYearMasterStateAsync(int targetYear, CancellationToken cancellationToken)
    {
        var state = await _repository.GetYearStateAsync(targetYear, cancellationToken);

        if (state is null)
        {
            throw new InvalidOperationException($"Target year {targetYear} does not exist in fps.tblyearmaster.");
        }

        var (yearStatus, active) = state.Value;

        if (!string.Equals(yearStatus, "Planned", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} is in status '{yearStatus}', expected 'Planned' before cutover.");
        }

        if (!active)
        {
            throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
        }
    }

    private async Task ValidateCopiedTableAsync(
        YearEndTableRuleMatrixEntry entry,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            return;
        }

        if (!await _repository.ColumnExistsAsync(entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var sourceCount = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", currentFpsYear, cancellationToken);
        var targetCount = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);

        switch (entry.FinalRowCountRule)
        {
            case YearEndFinalRowCountRule.MatchSource:
                if (targetCount != sourceCount)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} expected target-year row count to match source-year row count " +
                        $"(source={sourceCount}, target={targetCount}) for year {targetFpsYear}.");
                }

                break;

            case YearEndFinalRowCountRule.AtMostSource:
                if (targetCount > sourceCount)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} expected target-year row count to be at most source-year row count " +
                        $"(source={sourceCount}, target={targetCount}) for year {targetFpsYear}.");
                }

                break;

            case YearEndFinalRowCountRule.NotApplicable:
            default:
                throw new InvalidOperationException(
                    $"Table {entry.Schema}.{entry.TableName} is CopyToTargetYear but has no FinalRowCountRule — matrix authoring gap.");
        }
    }

    private async Task ValidateDedicatedStepTableHasTargetRowsAsync(
        YearEndTableRuleMatrixEntry entry,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            return;
        }

        if (!await _repository.ColumnExistsAsync(entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var count = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);
        if (count <= 0)
        {
            throw new InvalidOperationException(
                $"Expected target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear}, but found none.");
        }
    }

    private async Task ValidateExistsAsync(
        YearEndTableRuleMatrixEntry entry,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {entry.Schema}.{entry.TableName} does not exist.");
        }

        if (entry.Role == YearEndTableRole.GlobalReference)
        {
            // No fpsyear column, no target-year row concept — structural existence is the whole check.
            return;
        }

        if (!await _repository.ColumnExistsAsync(entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var count = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);
        if (count <= 0)
        {
            throw new InvalidOperationException(
                $"Expected target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear} (year-scoped dependency), but found none.");
        }
    }

    private async Task ValidateTargetYearIsEmptyAsync(
        YearEndTableRuleMatrixEntry entry,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        // Independent re-check, run at the very end of the pipeline — deliberately keeps its own
        // skip-if-missing behaviour (unlike ValidateTargetYearEmptyTablesStep, which treats a missing
        // table/column as a hard failure). This step is defense-in-depth, not the schema's contract
        // owner; ValidateYearScopedSchemaStep and ValidateTargetYearEmptyTablesStep already enforce
        // that guarantee earlier in the pipeline.
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            return;
        }

        var yearColumn = await _repository.ResolveYearColumnAsync(entry.Schema, entry.TableName, cancellationToken);
        if (yearColumn is null)
        {
            return;
        }

        await YearEndTargetYearEmptyPolicy.EnsureTargetYearIsEmptyAsync(_repository, entry, yearColumn, targetFpsYear, cancellationToken);
    }
}
