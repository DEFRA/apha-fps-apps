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
/// Extension methods for IServiceCollection to register infrastructure services.
/// Provides centralized service registration for dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services, database context, and application services.
    /// This is the central registration point for the AphaBatchJobs infrastructure layer.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance containing application settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required configuration sections are missing.</exception>
    public static IServiceCollection AddBatchJobsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind and register DatabaseOptions from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        }

        var databaseOptions = new DatabaseOptions
        {
            DefaultConnection = connectionString
        };

        // Bind additional database options if present in DatabaseOptions section
        var databaseOptionsSection = configuration.GetSection("DatabaseOptions");
        if (databaseOptionsSection.Exists())
        {
            databaseOptions.TimeoutSeconds = databaseOptionsSection.GetValue<int>("TimeoutSeconds", 30);
            databaseOptions.MaxRetries = databaseOptionsSection.GetValue<int>("MaxRetries", 3);
        }

        services.AddSingleton(databaseOptions);

        // Bind and register JobOptions from configuration using Options pattern
        services.Configure<JobOptions>(configuration.GetSection("JobOptions"));
        
        // Register JobOptions as singleton for backward compatibility if needed
        var jobOptions = new JobOptions();
        configuration.GetSection("JobOptions").Bind(jobOptions);
        services.AddSingleton(jobOptions);

        // Register AphaDbContext with Npgsql PostgreSQL provider
        services.AddDbContext<AphaDbContext>(options =>
        {
            options.UseNpgsql(
                databaseOptions.DefaultConnection,
                npgsqlOptions =>
                {
                    // Configure command timeout based on DatabaseOptions
                    npgsqlOptions.CommandTimeout(databaseOptions.TimeoutSeconds);
                    
                    // Enable retry on failure for transient errors (AWS RDS best practice)
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: databaseOptions.MaxRetries,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);

                    // Use split query behavior for better performance with complex queries
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    
                    // Set migrations assembly to ensure migrations are found
                    npgsqlOptions.MigrationsAssembly(typeof(AphaDbContext).Assembly.FullName);
                });

            // Enable sensitive data logging only in development (security best practice)
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging", false))
            {
                options.EnableSensitiveDataLogging();
            }

            // Enable detailed errors for better debugging (should be disabled in production)
            if (configuration.GetValue<bool>("Logging:EnableDetailedErrors", false))
            {
                options.EnableDetailedErrors();
            }
        });

        // Register CorrelationIdService as scoped instead of transient
        // Scoped lifetime ensures consistent correlation ID throughout the request/job scope
        services.AddScoped<ICorrelationIdService, CorrelationIdService>();

        // Register JobRunnerService as scoped
        // Scoped lifetime ensures one instance per job execution scope
        services.AddScoped<IJobRunnerService, JobRunnerService>();

        return services;
    }
}


**Key improvements made:**

1. **Connection string validation**: Separated the null/whitespace check for better clarity and proper validation before assignment.

2. **Options pattern**: Added `services.Configure<JobOptions>()` to follow the standard .NET Options pattern, while maintaining backward compatibility with the singleton registration.

3. **Migrations assembly**: Added `MigrationsAssembly` configuration to ensure EF Core can locate migrations properly.

4. **Service lifetime correction**: Changed `ICorrelationIdService` from Transient to Scoped. For correlation IDs in batch jobs, Scoped is more appropriate to maintain the same ID throughout a job execution scope.

5. **Code organization**: Improved the flow and readability by separating connection string validation from the DatabaseOptions instantiation.

6. **Comments**: Enhanced comments to clarify security and production considerations for logging settings.