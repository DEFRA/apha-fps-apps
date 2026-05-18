using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
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

        Assert.Equal("RecreateSummariesStepCatalog", catalog.GetType().Name);
        Assert.Equal("DotNetLinq", catalog.ImplementationName);
    }

    [Fact]
    public void ConfigureBatchJobServices_DefaultMabArchiveMode_ShouldRegisterSqlLoaders()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BatchJobsConnectionString"] = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123"
            })
            .Build();

        var services = new ServiceCollection();
        ServiceCollectionSetup.ConfigureBatchJobServices(services, config);

        using var serviceProvider = services.BuildServiceProvider();
        var loaders = serviceProvider.GetServices<IMabArchiveLoader>().ToList();

        Assert.Equal(24, loaders.Count);
        Assert.DoesNotContain(loaders, l => string.Equals(l.GetType().Name, "MyTlkpProgramDotNetLoader", StringComparison.Ordinal));
    }

    [Fact]
    public void ConfigureBatchJobServices_WhenMabArchiveModeIsDotNet_ShouldRegisterDotNetLoadersOnly()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BatchJobsConnectionString"] = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123",
                ["BatchJobs:MabArchiveImplementationMode"] = "DotNet"
            })
            .Build();

        var services = new ServiceCollection();
        ServiceCollectionSetup.ConfigureBatchJobServices(services, config);

        using var serviceProvider = services.BuildServiceProvider();
        var loaders = serviceProvider.GetServices<IMabArchiveLoader>().ToList();

        var ordered = loaders.OrderBy(l => l.Sequence).ToList();

        Assert.Equal(24, ordered.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, ordered.Select(l => l.Sequence));
        Assert.Equal(
            new[]
            {
                "my_tlkpprogram",
                "g_tlkpproject",
                "my_tlkpproject",
                "my_fpsyeartotals",
                "my_monthlyoutput",
                "my_monthlytime",
                "my_proj_invoice",
                "my_proj_subcontract",
                "my_projectmonthfinal",
                "my_tbladditionalcosts",
                "my_tblanimalreq",
                "my_tblcontract",
                "my_tblstaffjob",
                "my_timecostcalcs",
                "my_tlkptestreqmt",
                "tlkpyear",
                "my_workgroupgrade",
                "my_profitcentregrade",
                "my_tblprofitcentre",
                "my_testorproduct",
                "my_staff",
                "my_workgroup",
                "my_tblanimals",
                "my_tlkpproject_all"
            },
            ordered.Select(l => l.Name));
        Assert.All(ordered, l => Assert.Contains("DotNetLoader", l.GetType().Name, StringComparison.Ordinal));
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

