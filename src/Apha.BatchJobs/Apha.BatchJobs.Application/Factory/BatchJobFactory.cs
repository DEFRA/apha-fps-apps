using Apha.BatchJobs.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Factory;

/// <summary>
/// Batch job factory implementation that resolves job handlers from the DI container.
/// </summary>
public sealed class BatchJobFactory : IBatchJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the BatchJobFactory.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving job instances.</param>
    public BatchJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public IBatchJob Create(string jobName)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        var jobs = _serviceProvider.GetServices<IBatchJob>().ToList();
        var matches = jobs
            .Where(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
            throw new InvalidOperationException($"Job '{jobName}' is not registered. Available jobs: {string.Join(", ", jobs.Select(j => j.Name).Distinct(StringComparer.OrdinalIgnoreCase))}");

        if (matches.Count > 1)
            throw new InvalidOperationException($"Multiple job handlers are registered with Name='{jobName}'. Job names must be unique.");

        return matches[0];
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableJobs() =>
        _serviceProvider.GetServices<IBatchJob>()
            .Select(j => j.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
