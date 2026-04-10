using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Application.Services;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Infrastructure.Configuration;
using AphaBatchJobs.Infrastructure.Data;
using AphaBatchJobs.Infrastructure.Services;

namespace AphaBatchJobs.Infrastructure.DependencyInjection;

/// <summary>
/// Static class containing extension methods for IServiceCollection to register
/// all infrastructure, application, and core services required by the AphaBatchJobs solution.
/// This is the central dependency injection configuration point for the entire application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all batch job infrastructure services, database context, and application services
    /// into the dependency injection container. This method should be called from Program.cs
    /// during host configuration.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configuration">The IConfiguration instance containing application settings</param>
    /// <returns>The IServiceCollection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection string is missing</exception>
    /// <remarks>
    /// This method performs the following registrations:
    /// 1. Binds DatabaseOptions from configuration section 'DatabaseOptions'
    /// 2. Binds JobOptions from configuration section 'JobOptions'
    /// 3. Registers AphaDbContext with Npgsql PostgreSQL provider using ConnectionStrings:DefaultConnection
    /// 4. Registers ICorrelationIdService as CorrelationIdService with Transient lifetime
    /// 5. Registers IJobRunnerService as JobRunnerService with Scoped lifetime
    /// 
    /// All services are configured for optimal performance in AWS ECS Fargate containerized environments.
    /// </remarks>
    public static IServiceCollection AddBatchJobsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Validate input parameters using ArgumentNullException.ThrowIfNull (C# 11/.NET 7+ feature)
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind DatabaseOptions from configuration section 'DatabaseOptions'
        // This allows strongly-typed access to database configuration throughout the application
        services.Configure<DatabaseOptions>(
            configuration.GetSection("DatabaseOptions"));

        // Bind JobOptions from configuration section 'JobOptions'
        // This provides strongly-typed access to job execution configuration
        services.Configure<JobOptions>(
            configuration.GetSection("JobOptions"));

        // Register AphaDbContext with Npgsql PostgreSQL provider
        // Connection string is retrieved from ConnectionStrings:DefaultConnection
        // DbContext is registered with Scoped lifetime by default, which is appropriate for batch job scenarios
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Validate connection string exists to fail fast during startup
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' is not configured. " +
                "Ensure ConnectionStrings:DefaultConnection is set in appsettings.json or environment variables.");
        }

        services.AddDbContext<AphaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable retry on failure for transient fault handling in cloud environments
                // AWS ECS Fargate benefits from retry logic for network-related transient failures
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                // Set command timeout from configuration if available
                // Use GetSection().Get<T>() with null-conditional operator for safer access
                var databaseOptions = configuration.GetSection("DatabaseOptions").Get<DatabaseOptions>();
                if (databaseOptions?.TimeoutSeconds > 0)
                {
                    npgsqlOptions.CommandTimeout(databaseOptions.TimeoutSeconds);
                }

                // Set migration assembly to ensure migrations are found in Infrastructure project
                npgsqlOptions.MigrationsAssembly(typeof(AphaDbContext).Assembly.FullName);
            });

            // Enable sensitive data logging only in development for debugging
            // This should never be enabled in production ECS Fargate deployments
            // Use Environment.GetEnvironmentVariable for better compatibility with containerized environments
            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") 
                           ?? configuration.GetValue<string>("DOTNET_ENVIRONMENT") 
                           ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            
            var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
            
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // Configure query tracking behavior for better performance in batch scenarios
            // NoTracking is generally preferred for read-heavy batch operations
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        });

        // Register ICorrelationIdService with Transient lifetime
        // Transient ensures a new instance for each request, which is appropriate for correlation ID generation
        services.AddTransient<ICorrelationIdService, CorrelationIdService>();

        // Register IJobRunnerService with Scoped lifetime
        // Scoped lifetime ensures one instance per job execution scope, which is appropriate for orchestration
        services.AddScoped<IJobRunnerService, JobRunnerService>();

        // Return services for method chaining
        return services;
    }
}