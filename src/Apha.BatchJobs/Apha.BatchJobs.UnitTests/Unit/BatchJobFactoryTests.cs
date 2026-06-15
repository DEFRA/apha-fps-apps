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
        services.AddSingleton<IBatchJob, DummyBatchJob>();
        using var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);

        var job = factory.Create("Dummy");

        Assert.IsType<DummyBatchJob>(job);
        Assert.Equal("Dummy", job.Name);
    }

    [Fact]
    public void Create_ShouldThrowForUnknownJob()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider);

        var action = () => factory.Create("MissingJob");

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("MissingJob", exception.Message);
    }

    [Fact]
    public void GetAvailableJobs_ShouldReturnRegisteredNames()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBatchJob, DummyBatchJob>();
        services.AddSingleton<IBatchJob, ArchiveBatchJob>();
        using var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);

        Assert.Equal(new[] { "ArchiveJob", "Dummy" }, factory.GetAvailableJobs());
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

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("Multiple job handlers", exception.Message);
    }

    private sealed class DummyBatchJob : IBatchJob
    {
        public string Name => "Dummy";
        public string IdempotencyStrategy => "Upsert";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ArchiveBatchJob : IBatchJob
    {
        public string Name => "ArchiveJob";
        public string IdempotencyStrategy => "Checkpointing";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DuplicateNameJobA : IBatchJob
    {
        public string Name => "SameName";
        public string IdempotencyStrategy => "DedupKey";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DuplicateNameJobB : IBatchJob
    {
        public string Name => "SameName";
        public string IdempotencyStrategy => "DedupKey";

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
