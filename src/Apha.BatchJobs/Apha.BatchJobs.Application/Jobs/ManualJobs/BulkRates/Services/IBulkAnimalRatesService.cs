namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;

/// <summary>
/// Orchestration contract for the BulkAnimalRatesUpdate execution stream.
/// Applies Animal rate changes for an approved request.
/// </summary>
public interface IBulkAnimalRatesService
{
    /// <summary>
    /// Executes the full BulkAnimalRatesUpdate algorithm for the given approved context.
    /// </summary>
    Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default);
}
