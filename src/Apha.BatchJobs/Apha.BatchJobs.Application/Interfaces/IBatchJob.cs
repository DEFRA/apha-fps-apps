namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>Contract for all batch job implementations.</summary>
public interface IBatchJob
{
    /// <summary>Gets the unique name of the batch job.</summary>
    string Name { get; }

    /// <summary>Executes the batch job asynchronously.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}