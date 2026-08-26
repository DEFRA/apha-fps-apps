namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;

/// <summary>
/// Orchestration contract for the BulkStaffRatesUpdate execution stream.
/// Applies Staff profit-centre grade rate changes for an approved request.
/// </summary>
public interface IBulkStaffRatesService
{
    /// <summary>
    /// Executes the full BulkStaffRatesUpdate algorithm for the given approved context.
    /// </summary>
    Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default);
}
