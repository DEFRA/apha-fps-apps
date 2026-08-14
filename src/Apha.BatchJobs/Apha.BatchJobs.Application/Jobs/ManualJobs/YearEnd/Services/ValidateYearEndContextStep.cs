using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Validates mandatory Year End context values before data movement starts, deriving the current
/// FPS year live from <c>fps.tblyearmaster</c> rather than trusting a payload-supplied value.
/// </summary>
public sealed class ValidateYearEndContextStep : IYearEndDataSetupStep
{
    public string Name => "ValidateYearEndContextStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Data Setup requires plannedYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        var currentFpsYear = await YearEndYearContextResolver.ResolveCurrentFpsYearAsync(connection, transaction, cancellationToken);

        if (context.TargetFpsYear.Value == currentFpsYear)
        {
            throw new InvalidOperationException("plannedYear must be different from the current Open year.");
        }

        if (context.TargetFpsYear.Value < currentFpsYear)
        {
            throw new InvalidOperationException("plannedYear must be greater than the current Open year.");
        }

        return context with { CurrentFpsYear = currentFpsYear };
    }
}
