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

        // Resolve only the requested job type. No fallback: an unresolvable job name is a
        // registration/naming bug and must fail loudly here, not silently construct every
        // other registered job (which can fail for reasons unrelated to the requested job).
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
    /// Builds the conventional-name lookup from a set of candidate <see cref="IBatchJob"/> types.
    /// Internal (not private) so the collision-detection behavior is directly unit-testable
    /// without relying on reflection over the real production assembly.
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

        // No fallback here either: a naming collision between two job types must fail the
        // process at startup, not silently drop both from the fast-path map.
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
        // Keep the official job-name contract (job_master.jobname / BATCH_JOB_NAME / EventBridge
        // payloads) for handlers whose C# type name can't literally match it — a hyphen, or
        // historical capitalization, that a C# identifier can't contain. Phase 7D (main-port,
        // 2026-08-28): YearEnd-DataSetup/YearEnd-CutOver's hyphens were previously lost to the
        // generic Job/Handler/JobHandler-suffix stripping below, so BatchJobFactory.Create would
        // reject the real, hyphenated names used everywhere else in the system. A type-name-derived
        // guess must never be allowed to silently diverge from the real dispatch contract like that
        // again — checking the concrete type up front, before any stripping, closes that off.
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
