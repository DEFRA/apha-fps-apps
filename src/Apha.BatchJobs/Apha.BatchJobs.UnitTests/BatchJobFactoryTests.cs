using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Apha.BatchJobs.UnitTests;

public sealed class BatchJobFactoryTests
{
    [Fact]
    public void Create_ShouldResolveRegisteredJob()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBatchJob, DummyBatchJob>();
        using var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);

        var job = factory.Create("Dummy");

        job.ShouldBeOfType<DummyBatchJob>();
        job.Name.ShouldBe("Dummy");
    }

    [Fact]
    public void Create_ShouldThrowForUnknownJob()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider);

        var action = () => factory.Create("MissingJob");

        var exception = Should.Throw<InvalidOperationException>(action);
        exception.Message.ShouldContain("MissingJob");
    }

    [Fact]
    public void GetAvailableJobs_ShouldReturnRegisteredNames()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBatchJob, DummyBatchJob>();
        services.AddSingleton<IBatchJob, ArchiveBatchJob>();
        using var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);

        factory.GetAvailableJobs()
            .ShouldBe(new[] { "ArchiveJob", "Dummy" });
    }

    [Fact]
    public void Create_ShouldThrowForDuplicateJobNames()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBatchJob, DuplicateNameJobA>();
        services.AddSingleton<IBatchJob, DuplicateNameJobB>();
        using var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);

        var action = () => factory.Create("SameName");

        var exception = Should.Throw<InvalidOperationException>(action);
        exception.Message.ShouldContain("Multiple job handlers");
    }

    private sealed class DummyBatchJob : IBatchJob
    {
        public string Name => "Dummy";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ArchiveBatchJob : IBatchJob
    {
        public string Name => "ArchiveJob";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DuplicateNameJobA : IBatchJob
    {
        public string Name => "SameName";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DuplicateNameJobB : IBatchJob
    {
        public string Name => "SameName";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
