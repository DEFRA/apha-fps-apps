using Apha.BatchJobs.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Factory;

/// <summary>
/// Batch job factory implementation that resolves job handlers from the DI container.
/// </summary>
public sealed class BatchJobFactory : IBatchJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _jobRegistry;

    /// <summary>
    /// Initializes a new instance of the BatchJobFactory.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving job instances.</param>
    /// <param name="jobRegistry">Registry mapping job names to handler types.</param>
    public BatchJobFactory(IServiceProvider serviceProvider, Dictionary<string, Type> jobRegistry)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _jobRegistry = jobRegistry ?? throw new ArgumentNullException(nameof(jobRegistry));
    }

    /// <inheritdoc />
    public IBatchJob Create(string jobName)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (!_jobRegistry.TryGetValue(jobName, out var jobType))
            throw new InvalidOperationException($"Job '{jobName}' is not registered. Available jobs: {string.Join(", ", _jobRegistry.Keys)}");

        var instance = _serviceProvider.GetService(jobType) as IBatchJob;
        if (instance == null)
            throw new InvalidOperationException($"Failed to resolve job '{jobName}' from the service container.");

        return instance;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableJobs() => _jobRegistry.Keys;
}
