using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.UnitTests;

public sealed class ServiceCollectionSetupTests
{
    [Fact]
    public void CreateDefaultServices_ShouldRegisterExpectedFoundationServices()
    {
        var batchJobsRoot = GetBatchJobsRoot();
        var services = ServiceCollectionSetup.CreateDefaultServices(batchJobsRoot);
        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<IConfiguration>());

        var jobFactory = serviceProvider.GetRequiredService<IBatchJobFactory>();
        Assert.Contains("HealthCheck", jobFactory.GetAvailableJobs());
        Assert.Equal("HealthCheck", jobFactory.Create("HealthCheck").Name);
    }

    private static string GetBatchJobsRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var hasProject = File.Exists(Path.Combine(current.FullName, "BatchJobs.csproj"));
            var hasAppSettings = File.Exists(Path.Combine(current.FullName, "appsettings.json"));

            if (hasProject && hasAppSettings)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the BatchJobs project root.");
    }
}
