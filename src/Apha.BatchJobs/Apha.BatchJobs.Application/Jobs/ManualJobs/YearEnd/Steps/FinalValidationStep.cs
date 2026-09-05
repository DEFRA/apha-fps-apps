using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates final target-year setup state before Year End Data Setup completion. Matrix-driven —
/// dispatches validation per <see cref="YearEndTableRuleMatrix"/> entry by
/// <see cref="YearEndTableRuleMatrixEntry.FinalValidation"/>, not a second hardcoded table list.
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

    /// <summary>
    /// One entry's post-execution check: table (and its fpsyear column) must exist, then the
    /// target-year row count is checked against <see cref="YearEndTableRuleMatrixEntry.FinalValidation"/>.
    /// A missing table is a soft skip for <see cref="YearEndPrimaryRole.CopyToTargetYear"/> entries
    /// (matches <see cref="Steps.CopyFpsYearScopedTablesStep"/>'s own missing-table tolerance) and a
    /// hard failure for <see cref="YearEndPrimaryRole.TargetYearConfiguration"/>/
    /// <see cref="YearEndPrimaryRole.CreateTargetYear"/> entries, which Data Setup cannot proceed
    /// without.
    /// </summary>
    private async Task ValidateEntryAsync(
        YearEndTableRuleMatrixEntry entry,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await _repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
        {
            if (entry.PrimaryRole == YearEndPrimaryRole.CopyToTargetYear)
            {
                return;
            }

            throw new InvalidOperationException($"Required table {entry.Schema}.{entry.TableName} does not exist.");
        }

        if (!await _repository.ColumnExistsAsync(entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var targetCount = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);

        switch (entry.FinalValidation)
        {
            case YearEndFinalValidationRule.MatchSource:
            {
                var sourceCount = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", currentFpsYear, cancellationToken);
                if (targetCount != sourceCount)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} expected target-year row count to match source-year row count " +
                        $"(source={sourceCount}, target={targetCount}) for year {targetFpsYear}.");
                }

                break;
            }

            case YearEndFinalValidationRule.AtMostSource:
            {
                var sourceCount = await _repository.CountRowsByYearAsync(entry.Schema, entry.TableName, "fpsyear", currentFpsYear, cancellationToken);
                if (targetCount > sourceCount)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} expected target-year row count to be at most source-year row count " +
                        $"(source={sourceCount}, target={targetCount}) for year {targetFpsYear}.");
                }

                break;
            }

            case YearEndFinalValidationRule.ExactTargetRowCount:
                if (entry.ExpectedTargetRowCount is null)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} uses ExactTargetRowCount but has no ExpectedTargetRowCount — matrix authoring gap.");
                }

                if (targetCount != entry.ExpectedTargetRowCount.Value)
                {
                    throw new InvalidOperationException(
                        $"Expected exactly {entry.ExpectedTargetRowCount.Value} target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear}, but found {targetCount}.");
                }

                break;

            case YearEndFinalValidationRule.AtLeastOneTargetYearRow:
                if (targetCount <= 0)
                {
                    throw new InvalidOperationException(
                        $"Expected target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear}, but found none.");
                }

                break;

            default:
                throw new InvalidOperationException(
                    $"Matrix entry {entry.Schema}.{entry.TableName} has FinalValidation {entry.FinalValidation}, which final validation does not know how to check.");
        }
    }
}
