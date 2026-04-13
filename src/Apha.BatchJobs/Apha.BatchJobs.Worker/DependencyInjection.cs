using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.HealthCheck;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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

        // Register configuration sections
        var services = new ServiceCollection();

        services.Configure<DatabaseSettings>(config.GetSection("DatabaseConnection"));
        services.Configure<BatchJobSettings>(config.GetSection("BatchJobs"));
        services.Configure<ApplicationInsightsSettings>(config.GetSection("ApplicationInsights"));

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Apha.BatchJobs")
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(Log.Logger);
        });

        // Get database settings to build connection string
        var dbSettings = config.GetSection("DatabaseConnection").Get<DatabaseSettings>();
        if (dbSettings == null)
            throw new InvalidOperationException("DatabaseConnection configuration is missing.");

        // Register DbContext
        services.AddDbContext<BatchJobsDbContext>(options =>
        {
            options.UseNpgsql(
                dbSettings.BuildConnectionString(),
                npgsqlOptions => npgsqlOptions.CommandTimeout(dbSettings.Timeout));
        });

        // Register repositories
        services.AddScoped<IBatchLockRepository, BatchLockRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();

        // Register batch job handlers
        services.AddScoped<HealthCheckJobHandler>();

        // Create job registry
        var jobRegistry = new Dictionary<string, Type>
        {
            { "HealthCheck", typeof(HealthCheckJobHandler) }
        };

        // Register job factory
        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp, jobRegistry));

        // Register raw configuration for consumers that need direct access.
        services.AddSingleton<IConfiguration>(config);

        return services;



    }
}
