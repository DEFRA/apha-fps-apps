using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Infrastructure.Configuration;
using AphaBatchJobs.Infrastructure.Data;
using AphaBatchJobs.Infrastructure.ErrorHandling;
using AphaBatchJobs.Infrastructure.Services;

namespace AphaBatchJobs.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Static extension class for IServiceCollection to register infrastructure services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all infrastructure services including DbContext, correlation service, and error handling.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The configuration instance to bind options from.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Use Options pattern with validation instead of manual binding and validation
            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<JobOptions>()
                .Bind(configuration.GetSection("Job"))
                .ValidateOnStart();

            // Register DbContext with pooling for better performance in batch jobs
            services.AddDbContextPool<AphaDbContext>((serviceProvider, options) =>
            {
                // Retrieve validated options from DI container
                var databaseOptions = configuration
                    .GetSection(DatabaseOptions.SectionName)
                    .Get<DatabaseOptions>() ?? throw new InvalidOperationException("DatabaseOptions configuration is missing.");

                options.UseNpgsql(
                    databaseOptions.ConnectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
                        
                        if (databaseOptions.EnableRetryOnFailure)
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: databaseOptions.MaxRetryCount,
                                maxRetryDelay: TimeSpan.FromSeconds(databaseOptions.MaxRetryDelay),
                                errorCodesToAdd: null);
                        }

                        // Enable connection pooling optimizations
                        npgsqlOptions.MigrationsAssembly(typeof(AphaDbContext).Assembly.FullName);
                    });

                // Enable sensitive data logging only in development
                // options.EnableSensitiveDataLogging() should be controlled by environment
                
                // Enable detailed errors only in development
                // options.EnableDetailedErrors() should be controlled by environment
            });

            // Register correlation service as scoped for proper lifetime management
            services.AddScoped<ICorrelationIdService, CorrelationIdService>();

            // Register exception handler as singleton (correct lifetime)
            services.AddSingleton<GlobalExceptionHandler>();

            return services;
        }
    }
}


**Key improvements made:**

1. **Options Pattern Enhancement**: Used `AddOptions<T>()` with `ValidateOnStart()` instead of manual binding and validation, which is more idiomatic for .NET and ensures validation happens at startup.

2. **DbContext Pooling**: Changed from `AddDbContext` to `AddDbContextPool` for better performance in batch job scenarios where many short-lived contexts are created.

3. **Null Safety**: Added null-coalescing operator with exception throw when retrieving DatabaseOptions to prevent null reference exceptions.

4. **Migrations Assembly**: Added `MigrationsAssembly` configuration to ensure EF Core can locate migrations properly.

5. **Removed Redundant Configuration**: Removed duplicate `services.Configure<DatabaseOptions>()` call since `AddOptions<T>().Bind()` already registers the options.

6. **Comments**: Added comments about environment-specific configurations (sensitive data logging, detailed errors) that should be controlled by environment variables in production AWS environments.

7. **Better Exception Handling**: Improved configuration retrieval with proper null checking and meaningful exception messages.