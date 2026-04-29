using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Apha.BatchJobs.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Worker;

/// <summary>Bootstraps the DI service collection for the batch jobs worker.</summary>
public static class ServiceCollectionSetup
{
    /// <summary>Creates and returns the configured <see cref="IServiceCollection"/> for the worker host.</summary>
    /// <returns>A populated <see cref="IServiceCollection"/> ready for the host to build.</returns>
    public static IServiceCollection CreateDefaultServices(string? configurationBasePath = null)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = configurationBasePath ?? Directory.GetCurrentDirectory();
        
        // Build configuration from appsettings.json and environment variables
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        ConfigureBatchJobServices(services, config);

        return services;
    }

    /// <summary>Registers all batch job services, repositories and job handlers into the provided service collection.</summary>
    public static void ConfigureBatchJobServices(IServiceCollection services, IConfiguration config)
    {
        services.Configure<BatchJobSettings>(config.GetSection("BatchJobs"));
<<<<<<< HEAD
        services.Configure<ScheduledLoadFromFpsSettings>(config.GetSection("ScheduledLoadFromFps"));
        services.Configure<ApplicationInsightsSettings>(config.GetSection("ApplicationInsights"));
=======
>>>>>>> A-Foundation
        services.AddLogging();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var useInMemory = environment.Equals("Demo", StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
        {
            // Demo mode: Use in-memory implementations (no database required)
            services.AddScoped<IBatchLockRepository, NoDbBatchLockRepository>();
            services.AddScoped<IJobExecutionRepository, NoDbJobExecutionRepository>();
        }
        else
        {
            // Production mode: Use PostgreSQL database
            var connectionString = config.GetConnectionString("BatchJobsConnectionString")
                ?? throw new InvalidOperationException("Connection string 'BatchJobsConnectionString' not found.");

<<<<<<< HEAD
        services.AddScoped<IBatchLockRepository, BatchLockRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        services.AddSingleton<ICorrelationService, CorrelationService>();
        services.AddScoped<IScheduledLoadFromFpsPlanBuilder, ScheduledLoadFromFpsPlanBuilder>();
        services.AddScoped<IScheduledLoadFromFpsRepository, ScheduledLoadFromFpsRepository>();
        services.AddScoped<ICrossValidationEngine, CrossValidationEngine>();
        services.AddScoped<IScheduledLoadFromFpsStepHandler, ProcessPreviousYearTotalsHandler>();
        services.AddScoped<IScheduledLoadFromFpsStepHandler, ProcessCurrentYearTotalsHandler>();
        services.AddScoped<IScheduledLoadFromFpsStepHandler, DeleteYearsFpsDataHandler>();
        services.AddScoped<IScheduledLoadFromFpsStepHandler, AddYearsFpsDataHandler>();
        services.AddScoped<IScheduledLoadFromFpsStepHandler, HandleCurrentYearProjectAllHandler>();
=======
            services.AddDbContext<BatchJobsDbContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(30);
                    });
            });

            services.AddScoped<IBatchLockRepository, BatchLockRepository>();
            services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        }
>>>>>>> A-Foundation

        RegisterBatchJobs(services);
        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp));
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();
        services.AddSingleton(config);
    }

    private sealed class NoDbBatchLockRepository : IBatchLockRepository
    {
        public Task<bool> TryAcquireLockAsync(string jobName, string runId, int timeoutSeconds, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task ReleaseLockAsync(string jobName, string runId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default)
            => Task.FromResult<BatchLock?>(null);
    }

    private sealed class NoDbJobExecutionRepository : IJobExecutionRepository
    {
        public Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default)
            => Task.FromResult<JobExecutionRecord?>(null);
    }

    private static void RegisterBatchJobs(IServiceCollection services)
    {
        var batchJobType = typeof(IBatchJob);
        var applicationAssembly = batchJobType.Assembly;

        var jobTypes = applicationAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && batchJobType.IsAssignableFrom(t))
            .ToList();

        foreach (var jobType in jobTypes)
        {
            services.AddScoped(typeof(IBatchJob), jobType);
        }
    }
}
