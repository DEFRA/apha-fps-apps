using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Factory;

/// <summary>
/// Batch job factory implementation that resolves job handlers from the DI container.
/// </summary>
public sealed class BatchJobFactory : IBatchJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly IReadOnlyDictionary<string, Type> ConventionalJobMap =
        BuildConventionalJobMap(GetCandidateJobTypes(typeof(IBatchJob).Assembly));

    public BatchJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public IBatchJob Create(string jobName)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        // No fallback: an unresolvable job name is a registration bug and must fail loudly
        // here, not silently construct every other registered job.
        if (!ConventionalJobMap.TryGetValue(jobName, out var jobType))
        {
            throw new InvalidOperationException(
                $"Job '{jobName}' is not registered. Available jobs: {string.Join(", ", ConventionalJobMap.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}");
        }

        if (_serviceProvider.GetService(jobType) is not IBatchJob job)
        {
            throw new InvalidOperationException(
                $"Job '{jobName}' maps to {jobType.FullName} but it could not be resolved from the service container.");
        }

        return job;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableJobs() =>
        ConventionalJobMap.Keys
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static IEnumerable<Type> GetCandidateJobTypes(System.Reflection.Assembly assembly)
    {
        var batchJobType = typeof(IBatchJob);
        return assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                batchJobType.IsAssignableFrom(t));
    }

    /// <summary>
    /// Builds the conventional-name lookup from candidate <see cref="IBatchJob"/> types. Internal
    /// (not private) so collision detection is unit-testable without reflecting the real assembly.
    /// </summary>
    internal static IReadOnlyDictionary<string, Type> BuildConventionalJobMap(IEnumerable<Type> candidateTypes)
    {
        var grouped = candidateTypes
            .Select(t => new
            {
                Type = t,
                Name = GetConventionalName(t)
            })
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // A naming collision must fail startup, not silently drop both types from the map.
        var collisions = grouped.Where(g => g.Count() > 1).ToList();
        if (collisions.Count > 0)
        {
            var detail = string.Join("; ", collisions.Select(g =>
                $"'{g.Key}' -> {string.Join(", ", g.Select(x => x.Type.FullName))}"));
            throw new InvalidOperationException(
                $"Multiple IBatchJob types resolve to the same conventional name: {detail}. Job names must be unique.");
        }

        return grouped.ToDictionary(g => g.Key, g => g.Single().Type, StringComparer.OrdinalIgnoreCase);
    }

    internal static string GetConventionalName(Type type)
    {
        // Explicit overrides for names a C# identifier can't hold (hyphens). Generic suffix
        // stripping below would otherwise drop YearEnd-DataSetup/YearEnd-CutOver's hyphens and
        // reject the real dispatch names used everywhere else in the system.
        if (type == typeof(MabArchiveJob))
            return BatchJobNames.MabArchive;

        if (type == typeof(YearEndDataSetupJobHandler))
            return BatchJobNames.YearEndDataSetup;

        if (type == typeof(YearEndCutoverJobHandler))
            return BatchJobNames.YearEndCutover;

        var name = type.Name;

        if (name.EndsWith("JobHandler", StringComparison.Ordinal))
            name = name[..^"JobHandler".Length];
        else if (name.EndsWith("Handler", StringComparison.Ordinal))
            name = name[..^"Handler".Length];
        else if (name.EndsWith("Job", StringComparison.Ordinal))
            name = name[..^"Job".Length];

        return name;
    }
}
