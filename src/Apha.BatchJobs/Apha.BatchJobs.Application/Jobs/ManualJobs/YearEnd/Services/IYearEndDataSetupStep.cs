using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Represents one ordered step in the Year End Data Setup pipeline. All steps share the single
/// connection/transaction owned by <see cref="YearEndDataSetupService"/> — no step may open its
/// own connection for Year End business work.
/// </summary>
public interface IYearEndDataSetupStep
{
    /// <summary>
    /// Human-readable step name used in structured logs.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the step against the shared connection/transaction and returns the (possibly
    /// updated) context to carry forward into the next step.
    /// </summary>
    Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default);
}
