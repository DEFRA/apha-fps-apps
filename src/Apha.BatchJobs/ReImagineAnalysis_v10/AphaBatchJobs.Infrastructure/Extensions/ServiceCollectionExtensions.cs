using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Application.Services;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Infrastructure.Configuration;
using AphaBatchJobs.Infrastructure.Data;
using AphaBatchJobs.Infrastructure.Services;

namespace AphaBatchJobs.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring batch jobs infrastructure services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all batch jobs infrastructure services to the service collection.
    /// Configures database context, options, and core services required for batch job execution.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance containing connection strings and options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection string is missing or invalid.</exception>
    /// <remarks>
    /// This method performs the following registrations:
    /// - Binds DatabaseOptions from configuration section "DatabaseOptions"
    /// - Binds JobOptions from configuration section "JobOptions"
    /// - Registers AphaDbContext with PostgreSQL provider using DefaultConnection
    /// - Registers CorrelationIdService as ICorrelationIdService (transient)
    /// - Registers JobRunnerService as IJobRunnerService (scoped)
    /// 
    /// For AWS deployments, ensure connection strings are properly configured:
    /// - Use AWS Secrets Manager for production credentials
    /// - Configure appropriate timeout values for network latency
    /// - Set retry policies suitable for cloud environments
    /// </remarks>
    public static IServiceCollection AddBatchJobsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind DatabaseOptions from configuration section "DatabaseOptions"
        // Use BindConfiguration for .NET 8 best practice
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration("DatabaseOptions")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Bind JobOptions from configuration section "JobOptions"
        services.AddOptions<JobOptions>()
            .BindConfiguration("JobOptions")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register AphaDbContext with PostgreSQL provider
        // Uses DefaultConnection from ConnectionStrings section
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Validate connection string exists
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<AphaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // AWS best practice: Enable retry on failure for transient errors
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                // PostgreSQL best practice: Set command timeout from configuration
                var timeoutSeconds = configuration
                    .GetSection("DatabaseOptions:TimeoutSeconds")
                    .Get<int?>();
                
                if (timeoutSeconds.HasValue && timeoutSeconds.Value > 0)
                {
                    npgsqlOptions.CommandTimeout(timeoutSeconds.Value);
                }

                // AWS best practice: Set migration assembly for proper deployment
                npgsqlOptions.MigrationsAssembly(typeof(AphaDbContext).Assembly.FullName);
            });

            // .NET 8 best practice: Enable sensitive data logging only in development
            var enableSensitiveDataLogging = configuration
                .GetValue<bool>("Logging:EnableSensitiveDataLogging", false);
            
            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            // .NET 8 best practice: Enable detailed errors in development
            var enableDetailedErrors = configuration
                .GetValue<bool>("Logging:EnableDetailedErrors", false);
            
            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            // PostgreSQL best practice: Configure query splitting behavior
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Register CorrelationIdService as transient
        // Transient lifetime ensures a new instance for each request, suitable for stateless ID generation
        services.AddTransient<ICorrelationIdService, CorrelationIdService>();

        // Register JobRunnerService as scoped
        // Scoped lifetime ensures one instance per job execution scope, appropriate for orchestration
        services.AddScoped<IJobRunnerService, JobRunnerService>();

        return services;
    }
}
