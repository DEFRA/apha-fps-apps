using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
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
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        ConfigureBatchJobServices(services, config);

        return services;
    }

    internal static void ConfigureBatchJobServices(IServiceCollection services, IConfiguration config)
    {
        services.Configure<BatchJobSettings>(config.GetSection("BatchJobs"));
        services.Configure<ApplicationInsightsSettings>(config.GetSection("ApplicationInsights"));
        services.AddLogging();

        var connectionString = config.GetConnectionString("BatchJobsConnectionString")
            ?? throw new InvalidOperationException("Connection string 'BatchJobsConnectionString' not found.");

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

        RegisterBatchJobs(services);
        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp));
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();
        services.AddSingleton(config);
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
