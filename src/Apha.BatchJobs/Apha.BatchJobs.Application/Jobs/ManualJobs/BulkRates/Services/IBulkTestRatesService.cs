namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;

/// <summary>
/// Orchestration contract for the BulkTestRatesUpdate execution stream.
/// Applies FEC Test/Product and AGRUP rate changes for an approved request.
/// </summary>
public interface IBulkTestRatesService
{
    /// <summary>
    /// Executes the full BulkTestRatesUpdate algorithm for the given approved context.
    /// </summary>
    Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default);
}
