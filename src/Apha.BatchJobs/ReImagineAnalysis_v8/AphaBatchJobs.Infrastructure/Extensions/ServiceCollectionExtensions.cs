using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Application.Services;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Infrastructure.Data;
using AphaBatchJobs.Infrastructure.Options;
using AphaBatchJobs.Infrastructure.Services;

namespace AphaBatchJobs.Infrastructure.Extensions;

/// <summary>
/// Extension class for IServiceCollection that registers all infrastructure services.
/// This class provides a centralized configuration point for dependency injection setup,
/// including database context, options binding, and core services required for batch job execution.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services required for the AphaBatchJobs application.
    /// This includes database context with Npgsql provider, configuration options binding,
    /// and core service implementations for job execution and correlation tracking.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The application configuration containing connection strings and options.</param>
    /// <returns>The IServiceCollection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection string is missing or empty.</exception>
    /// <remarks>
    /// This method performs the following registrations:
    /// 1. Binds DatabaseOptions from the "DatabaseOptions" configuration section
    /// 2. Binds JobOptions from the "JobOptions" configuration section
    /// 3. Registers AphaDbContext with Npgsql using the DefaultConnection connection string
    /// 4. Registers CorrelationIdService as ICorrelationIdService with transient lifetime
    /// 5. Registers JobRunnerService as IJobRunnerService with scoped lifetime
    /// 
    /// For AWS deployments, ensure connection strings are stored securely using AWS Secrets Manager
    /// or Parameter Store and injected into configuration at runtime.
    /// </remarks>
    public static IServiceCollection AddBatchJobsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind DatabaseOptions from configuration section "DatabaseOptions"
        // Use BindConfiguration for better performance and validation support in .NET 8
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration("DatabaseOptions")
            .ValidateOnStart();

        // Bind JobOptions from configuration section "JobOptions"
        services.AddOptions<JobOptions>()
            .BindConfiguration("JobOptions")
            .ValidateOnStart();

        // Register AphaDbContext with Npgsql provider using DefaultConnection connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Validate connection string exists
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' is not configured. " +
                "Ensure the connection string is set in configuration or AWS Secrets Manager.");
        }

        services.AddDbContext<AphaDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    // Enable retry on failure for transient errors (important for AWS RDS)
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);

                    // Set command timeout from configuration
                    var dbOptions = configuration.GetSection("DatabaseOptions").Get<DatabaseOptions>();
                    if (dbOptions?.TimeoutSeconds > 0)
                    {
                        npgsqlOptions.CommandTimeout(dbOptions.TimeoutSeconds);
                    }

                    // Use split query behavior for better performance with complex queries
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    
                    // Set migration assembly to ensure migrations are found
                    npgsqlOptions.MigrationsAssembly(typeof(AphaDbContext).Assembly.FullName);
                });

            // Enable sensitive data logging only in development
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }

            // Enable detailed errors only in development
            if (configuration.GetValue<bool>("Logging:EnableDetailedErrors"))
            {
                options.EnableDetailedErrors();
            }
        });

        // Register CorrelationIdService as ICorrelationIdService with transient lifetime
        // Transient lifetime ensures a new instance is created each time it's requested,
        // which is appropriate for a stateless service that generates unique identifiers
        services.AddTransient<ICorrelationIdService, CorrelationIdService>();

        // Register JobRunnerService as IJobRunnerService with scoped lifetime
        // Scoped lifetime ensures one instance per request/operation scope,
        // which is appropriate for orchestrating job execution within a single batch run
        services.AddScoped<IJobRunnerService, JobRunnerService>();

        return services;
    }
}