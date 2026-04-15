using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using FluentAssertions;
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

        job.Should().BeOfType<DummyBatchJob>();
        job.Name.Should().Be("Dummy");
    }

    [Fact]
    public void Create_ShouldThrowForUnknownJob()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider, new Dictionary<string, Type>());

        var action = () => factory.Create("MissingJob");

        var exception = action.Should().Throw<InvalidOperationException>().Which;
        exception.Message.Should().Contain("MissingJob");
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

        factory.GetAvailableJobs().OrderBy(name => name)
            .Should().Equal("ArchiveJob", "HealthCheck");
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
