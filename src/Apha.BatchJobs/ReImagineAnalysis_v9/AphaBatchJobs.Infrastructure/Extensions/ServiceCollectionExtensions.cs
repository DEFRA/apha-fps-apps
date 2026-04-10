using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Application.Services;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Infrastructure.Data;
using AphaBatchJobs.Infrastructure.Options;
using AphaBatchJobs.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AphaBatchJobs.Infrastructure.Extensions;

/// <summary>
/// Static extension class for registering all infrastructure services in dependency injection container.
/// Provides configuration and registration of database context, options, and core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services including database context, options, and job services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance containing application settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection string is missing.</exception>
    public static IServiceCollection AddBatchJobsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind and register DatabaseOptions from configuration
        var databaseOptionsSection = configuration.GetSection("DatabaseOptions");
        services.Configure<DatabaseOptions>(databaseOptionsSection);
        
        // Get DatabaseOptions for immediate use in DbContext configuration
        var databaseOptions = databaseOptionsSection.Get<DatabaseOptions>() ?? new DatabaseOptions();

        // Bind and register JobOptions from configuration
        services.Configure<JobOptions>(configuration.GetSection("JobOptions"));

        // Register AphaDbContext with Npgsql provider using DefaultConnection
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
                // Set command timeout from configuration
                npgsqlOptions.CommandTimeout(databaseOptions.TimeoutSeconds);
                
                // Configure retry logic for transient failures (important for AWS RDS)
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: databaseOptions.MaxRetries,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                
                // Enable migration assembly if needed for proper deployment
                npgsqlOptions.MigrationsAssembly(typeof(AphaDbContext).Assembly.FullName);
            });
            
            // Enable sensitive data logging only in development (security best practice)
            // options.EnableSensitiveDataLogging(isDevelopment);
            
            // Enable detailed errors only in development (performance best practice)
            // options.EnableDetailedErrors(isDevelopment);
        });

        // Register CorrelationIdService as ICorrelationIdService with transient lifetime
        services.AddTransient<ICorrelationIdService, CorrelationIdService>();

        // Register JobRunnerService as IJobRunnerService with scoped lifetime
        services.AddScoped<IJobRunnerService, JobRunnerService>();

        return services;
    }
}


**Key improvements made:**

1. **Connection String Validation**: Added null/whitespace check for connection string with descriptive exception message to fail fast during startup.

2. **DatabaseOptions Retrieval**: Changed from manual `Bind()` to `Get<DatabaseOptions>()` which is more idiomatic and handles null cases better.

3. **Migrations Assembly Configuration**: Added `MigrationsAssembly` configuration to ensure EF Core migrations work correctly when the DbContext is in a different assembly.

4. **Exception Documentation**: Added `InvalidOperationException` to XML documentation for better API clarity.

5. **Code Comments**: Added inline comments for AWS RDS retry logic to clarify the purpose.

6. **Configuration Reuse**: Stored `databaseOptionsSection` in a variable to avoid calling `GetSection()` twice.

7. **Commented Best Practices**: Added commented lines for `EnableSensitiveDataLogging` and `EnableDetailedErrors` as reminders for environment-specific configuration (these should be enabled based on environment detection in production code).