using System;
using AphaBatchJobsFoundationV3.Host.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace AphaBatchJobsFoundationV3.Host.Extensions
{
    /// <summary>
    /// Extension methods for registering Host layer services in the dependency injection container.
    /// Provides configuration for CLI executor, hosted services, and structured logging with correlation ID enrichment.
    /// </summary>
    public static class HostDependencyInjection
    {
        /// <summary>
        /// Registers Host layer services in the dependency injection container.
        /// Configures CLI job executor as transient and scheduler hosted service.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public static IServiceCollection AddHostServices(this IServiceCollection services)
        {
            // Use ArgumentNullException.ThrowIfNull for more concise null checking (available in .NET 6+)
            // If targeting earlier versions, the original pattern is acceptable
            ArgumentNullException.ThrowIfNull(services);

            // Register CLI job executor as transient for per-execution isolation
            services.AddTransient<CliJobExecutor>();

            // Register scheduler hosted service for background job scheduling
            services.AddHostedService<SchedulerHostedService>();

            return services;
        }

        /// <summary>
        /// Configures Serilog structured logging with correlation ID enrichment.
        /// Sets up console sink with structured output template and environment enrichment.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure logging for.</param>
        /// <returns>The host builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when hostBuilder is null.</exception>
        public static IHostBuilder AddStructuredLogging(this IHostBuilder hostBuilder)
        {
            // Use ArgumentNullException.ThrowIfNull for more concise null checking (available in .NET 6+)
            // If targeting earlier versions, the original pattern is acceptable
            ArgumentNullException.ThrowIfNull(hostBuilder);

            hostBuilder.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", "AphaBatchJobs")
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information);
            });

            return hostBuilder;
        }
    }
}


// Changes made:
// 1. Replaced manual null checks with ArgumentNullException.ThrowIfNull() for more idiomatic .NET 6+ code
//    - This is more concise and follows modern .NET conventions
//    - If targeting .NET 5 or earlier, revert to the original null check pattern
// 2. All other aspects of the code follow .NET best practices:
//    - Proper use of extension methods for dependency injection
//    - Appropriate service lifetimes (Transient for CliJobExecutor, Hosted Service for background work)
//    - Well-structured XML documentation comments
//    - Proper method chaining pattern for fluent API design
//    - Consistent naming conventions and code organization