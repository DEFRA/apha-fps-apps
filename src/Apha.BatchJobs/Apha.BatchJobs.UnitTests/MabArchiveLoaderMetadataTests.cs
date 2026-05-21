using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

namespace Apha.BatchJobs.UnitTests;

public class MabArchiveLoaderMetadataTests
{
    [Fact]
    public void LoaderMetadata_MatchesExpectedLegacyCoverage()
    {
        var loaderTypes = typeof(IMyFpsYearlyDataService).Assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                typeof(IMabArchiveLoader).IsAssignableFrom(t) &&
                t.Namespace == "Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders")
            .ToList();

        static bool InheritsFromLinqLoaderBase(Type type)
        {
            var current = type.BaseType;
            while (current is not null)
            {
                if (string.Equals(current.Name, "MabArchiveLinqLoaderBase", StringComparison.Ordinal))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        var linqLoaderTypes = loaderTypes
            .Where(InheritsFromLinqLoaderBase)
            .ToList();

        Assert.Equal(24, linqLoaderTypes.Count);

        var loaders = linqLoaderTypes
            .Select(t => (IMabArchiveLoader)Activator.CreateInstance(t)!)
            .OrderBy(l => l.Sequence)
            .ToList();

        Assert.Equal(Enumerable.Range(1, 24), loaders.Select(l => l.Sequence));
        Assert.Equal(24, loaders.Select(l => l.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.DoesNotContain(loaders, l => string.IsNullOrWhiteSpace(l.Name));
    }
}
