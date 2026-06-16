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
                ["ConnectionStrings:FPSConnectionString"] = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD"
            })
            .Build();

        var services = new ServiceCollection();
        ServiceCollectionSetup.ConfigureBatchJobServices(services, config);

        using var serviceProvider = services.BuildServiceProvider();
        var catalog = serviceProvider.GetRequiredService<Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries.IRecreateSummariesStepCatalog>();

        Assert.Equal("RecreateSummariesStepCatalog", catalog.GetType().Name);
    }

    [Fact]
    public void ConfigureBatchJobServices_DefaultMabArchiveMode_ShouldRegisterMabArchiveLoadersOnly()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FPSConnectionString"] = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD"
            })
            .Build();

        var services = new ServiceCollection();
        ServiceCollectionSetup.ConfigureBatchJobServices(services, config);

        using var serviceProvider = services.BuildServiceProvider();
        var loaders = serviceProvider.GetServices<IMabArchiveLoader>().ToList();

        Assert.Equal(24, loaders.Count);
        Assert.All(loaders, l => Assert.EndsWith("Loader", l.GetType().Name, StringComparison.Ordinal));
    }

    [Fact]
    public void ConfigureBatchJobServices_WhenMabArchiveModeIsConfigured_ShouldStillRegisterMabArchiveLoadersOnly()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FPSConnectionString"] = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD",
                ["BatchJobs:MabArchiveImplementationMode"] = "Sql"
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
        Assert.All(ordered, l => Assert.EndsWith("Loader", l.GetType().Name, StringComparison.Ordinal));
    }

    private static string GetBatchJobsRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            // Look for Worker project directory which contains appsettings.json
            var workerPath = Path.Combine(current.FullName, "Apha.BatchJobs.Worker");
            if (Directory.Exists(workerPath) && File.Exists(Path.Combine(workerPath, "appsettings.json")))
            {
                return workerPath;
            }

            current = current.Parent;
        }
        
        // Try relative path from test execution directory
        var testDir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && testDir?.Parent != null; i++)
        {
            testDir = testDir.Parent;
            var candidate = Path.Combine(testDir.FullName, "src", "Apha.BatchJobs", "Apha.BatchJobs.Worker");
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the Apha.BatchJobs.Worker directory with appsettings.json.");
    }
}

