using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Shared check that a table has zero target-year rows. Used by both
/// <see cref="Steps.ValidateTargetYearEmptyTablesStep"/> and <see cref="Steps.FinalValidationStep"/>
/// so the check can't drift between the two. Assumes the table/column already exist.
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
