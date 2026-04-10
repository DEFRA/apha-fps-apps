using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Extensions.DependencyInjection;
using AphaBatchJobsFoundationV3.Core.Interfaces;
using AphaBatchJobsFoundationV3.Infrastructure.Data;
using AphaBatchJobsFoundationV3.Infrastructure.Scheduling;
using AphaBatchJobsFoundationV3.Infrastructure.Services;

namespace AphaBatchJobsFoundationV3.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for registering Infrastructure layer services in dependency injection container.
    /// Configures DbContext with PostgreSQL, CorrelationService, and Quartz.NET scheduler.
    /// Provides centralized infrastructure service registration aligned to Apha conventions.
    /// </summary>
    public static class InfrastructureDependencyInjection
    {
        /// <summary>
        /// Static extension method on IServiceCollection accepting IConfiguration parameter.
        /// Registers BatchJobDbContext with PostgreSQL using connection string from configuration,
        /// registers CorrelationService as singleton implementing ICorrelationService,
        /// registers QuartzJobScheduler as singleton implementing IJobScheduler,
        /// calls AddQuartzScheduler method.
        /// </summary>
        /// <param name="services">The service collection to add infrastructure services to.</param>
        /// <param name="configuration">The configuration instance containing connection strings and settings.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when services or configuration is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when connection string is not found in configuration.
        /// </exception>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Register BatchJobDbContext with PostgreSQL
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            services.AddDbContext<BatchJobDbContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(30);
                    });
                
                // Enable sensitive data logging only in development
                var isDevelopment = configuration.GetValue<bool>("IsDevelopment");
                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Register CorrelationService as singleton implementing ICorrelationService
            services.AddSingleton<ICorrelationService, CorrelationService>();

            // Register QuartzJobScheduler as singleton implementing IJobScheduler
            services.AddSingleton<IJobScheduler, QuartzJobScheduler>();

            // Configure Quartz.NET scheduler
            services.AddQuartzScheduler();

            return services;
        }

        /// <summary>
        /// Private static method on IServiceCollection to configure Quartz.NET services.
        /// Adds Quartz using AddQuartz with configuration for in-memory job store,
        /// registers QuartzJobWrapper as transient, configures scheduler factory.
        /// </summary>
        /// <param name="services">The service collection to add Quartz services to.</param>
        /// <returns>The service collection for method chaining.</returns>
        private static IServiceCollection AddQuartzScheduler(this IServiceCollection services)
        {
            // Add Quartz services with configuration
            services.AddQuartz(quartzConfig =>
            {
                // Use unique scheduler instance name
                quartzConfig.SchedulerId = "AphaBatchJobsScheduler";
                quartzConfig.SchedulerName = "Apha Batch Jobs Scheduler";

                // Use in-memory job store for foundation
                quartzConfig.UseInMemoryStore();

                // Use default thread pool with 10 threads
                quartzConfig.UseDefaultThreadPool(threadPool =>
                {
                    threadPool.MaxConcurrency = 10;
                });

                // Configure job execution settings
                quartzConfig.InterruptJobsOnShutdown = true;
                quartzConfig.InterruptJobsOnShutdownWithWait = true;
            });

            // Add Quartz hosted service for automatic start/stop
            services.AddQuartzHostedService(options =>
            {
                // Wait for jobs to complete on shutdown
                options.WaitForJobsToComplete = true;
                
                // Await running jobs on shutdown
                options.AwaitApplicationStarted = true;
            });

            return services;
        }
    }
}


// Key improvements made:
// 1. Replaced 'System.ArgumentNullException' with 'ArgumentNullException' (no need for fully qualified name with proper using)
// 2. Used 'ArgumentNullException.ThrowIfNull()' instead of manual null checks (modern .NET pattern, available in .NET 6+)
// 3. Replaced 'System.TimeSpan' with 'TimeSpan' (consistent with other type references)
// 4. Added validation for connection string to fail fast with clear error message
// 5. Updated XML documentation to use 'ArgumentNullException' without 'System.' prefix
// 6. Added 'InvalidOperationException' to XML documentation for connection string validation
// 7. Improved code consistency and readability while maintaining all existing functionality
