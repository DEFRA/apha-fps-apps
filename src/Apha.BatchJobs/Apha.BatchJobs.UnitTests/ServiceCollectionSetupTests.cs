using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.DependencyInjection;
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

    [Fact]
    public void CreateDefaultServices_AllRegisteredJobs_ShouldDeclareExplicitIdempotencyStrategy()
    {
        var batchJobsRoot = GetBatchJobsRoot();
        var services = ServiceCollectionSetup.CreateDefaultServices(batchJobsRoot);
        using var serviceProvider = services.BuildServiceProvider();

        var jobs = serviceProvider.GetServices<IBatchJob>().ToList();
        Assert.NotEmpty(jobs);

        foreach (var job in jobs)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(job.IdempotencyStrategy),
                $"Job '{job.Name}' must declare a non-empty idempotency strategy.");
        }
    }

    [Fact]
    public void CreateDefaultServices_ManualAdhocJobs_ShouldHaveNoScheduleExpression()
    {
        var batchJobsRoot = GetBatchJobsRoot();
        var services = ServiceCollectionSetup.CreateDefaultServices(batchJobsRoot);
        using var serviceProvider = services.BuildServiceProvider();

        var jobs = serviceProvider.GetServices<IBatchJob>().ToList();

        var manualJobNames = new[] { "HealthCheck", "FECProcess", "RecreateSummaries" };
        foreach (var jobName in manualJobNames)
        {
            var matchingJobs = jobs.Where(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.Single(matchingJobs);
            Assert.Null(matchingJobs[0].ScheduleExpression);
        }
    }

    [Fact]
    public void ConfigureBatchJobServices_WhenRecreateSummariesModeIsDotNetLinq_ShouldResolveLinqCatalog()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BatchJobsConnectionString"] = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123",
                ["BatchJobs:RecreateSummariesImplementationMode"] = "DotNetLinq"
            })
            .Build();

        var services = new ServiceCollection();
        ServiceCollectionSetup.ConfigureBatchJobServices(services, config);

        using var serviceProvider = services.BuildServiceProvider();
        var catalog = serviceProvider.GetRequiredService<Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries.IRecreateSummariesStepCatalog>();

        Assert.Equal("LinqRecreateSummariesStepCatalog", catalog.GetType().Name);
        Assert.Equal("DotNetLinq", catalog.ImplementationName);
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

