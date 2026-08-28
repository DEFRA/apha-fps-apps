using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Shared policy check for <see cref="YearEndTableRuleAction.TargetYearMustBeEmpty"/> matrix entries:
/// the target year must have zero rows in this table. Used by both
/// <see cref="Steps.ValidateTargetYearEmptyTablesStep"/> (runs mid-pipeline) and
/// <see cref="Steps.FinalValidationStep"/> (independent re-check at the end of the pipeline) so the
/// same policy can't drift between the two call sites. Callers are responsible for resolving
/// <c>yearColumn</c> and deciding what to do if the table/column doesn't exist — this
/// method assumes both are already known-good and only enforces the row-count policy itself.
/// </summary>
internal static class YearEndTargetYearEmptyPolicy
{
    public static async Task EnsureTargetYearIsEmptyAsync(
        IYearEndDataSetupRepository repository,
        YearEndTableRuleMatrixEntry entry,
        string yearColumn,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        var count = await repository.CountRowsByYearAsync(entry.Schema, entry.TableName, yearColumn, targetFpsYear, cancellationToken);
        if (count != 0)
        {
            throw new InvalidOperationException(
                $"Expected no target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear}, but found {count}.");
        }
    }
}
