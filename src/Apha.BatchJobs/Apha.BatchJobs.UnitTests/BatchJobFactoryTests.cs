using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.UnitTests;

public sealed class BatchJobFactoryTests
{
    [Fact]
    public void Create_ShouldResolveRegisteredJob()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DummyBatchJob>();
        using var serviceProvider = services.BuildServiceProvider();

        var registry = new Dictionary<string, Type>
        {
            ["Dummy"] = typeof(DummyBatchJob)
        };

        var factory = new BatchJobFactory(serviceProvider, registry);

        var job = factory.Create("Dummy");

        Assert.IsType<DummyBatchJob>(job);
        Assert.Equal("Dummy", job.Name);
    }

    [Fact]
    public void Create_ShouldThrowForUnknownJob()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider, new Dictionary<string, Type>());

        var action = () => factory.Create("MissingJob");

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("MissingJob", exception.Message);
    }

    [Fact]
    public void GetAvailableJobs_ShouldReturnRegisteredNames()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider, new Dictionary<string, Type>
        {
            ["HealthCheck"] = typeof(DummyBatchJob),
            ["ArchiveJob"] = typeof(DummyBatchJob)
        });

        Assert.Equal(
            ["ArchiveJob", "HealthCheck"],
            factory.GetAvailableJobs().OrderBy(name => name));
    }

    private sealed class DummyBatchJob : IBatchJob
    {
        public string Name => "Dummy";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
