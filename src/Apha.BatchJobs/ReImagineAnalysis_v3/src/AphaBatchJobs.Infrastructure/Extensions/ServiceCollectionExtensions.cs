using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using AphaBatchJobs.Infrastructure.Configuration;
using AphaBatchJobs.Infrastructure.Logging;
using AphaBatchJobs.Infrastructure.ErrorHandling;

namespace AphaBatchJobs.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for dependency injection setup of infrastructure layer services.
    /// Provides centralized registration of logging, error handling, database configuration,
    /// and options binding for the Apha BatchJobs infrastructure layer.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all infrastructure services including logging, error handling, database configuration,
        /// and job configuration. This is the main entry point for infrastructure layer dependency injection.
        /// </summary>
        /// <param name="services">The service collection to add infrastructure services to.</param>
        /// <param name="configuration">The application configuration containing settings for infrastructure services.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Register infrastructure services in logical order
            services.AddStructuredLogging(configuration);
            services.AddErrorHandling();
            services.AddDatabaseConfiguration(configuration);
            services.AddJobConfiguration(configuration);

            return services;
        }

        /// <summary>
        /// Configures Serilog structured logging with console and file sinks.
        /// Binds LoggingOptions from configuration and registers correlation id services
        /// and structured logger as singletons for consistent logging across the application.
        /// </summary>
        /// <param name="services">The service collection to add logging services to.</param>
        /// <param name="configuration">The application configuration containing logging settings.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
        public static IServiceCollection AddStructuredLogging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Bind and validate LoggingOptions from configuration
            var loggingOptions = configuration
                .GetSection(LoggingOptions.SectionName)
                .Get<LoggingOptions>() ?? new LoggingOptions();

            // Validate logging options
            if (!loggingOptions.Validate())
            {
                throw new InvalidOperationException(
                    "Invalid logging configuration. Please check appsettings.json Logging section.");
            }

            // Register LoggingOptions as singleton
            services.AddSingleton(loggingOptions);

            // Parse minimum log level from configuration
            var minimumLevel = Enum.TryParse<LogEventLevel>(loggingOptions.MinimumLevel, true, out var level)
                ? level
                : LogEventLevel.Information;

            // Configure Serilog
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId();

            // Add console sink if enabled
            if (loggingOptions.EnableConsoleLogging)
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
            }

            // Add file sink if enabled
            if (loggingOptions.EnableFileLogging)
            {
                loggerConfiguration.WriteTo.File(
                    path: "logs/apha-batchjobs-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 104857600, // 100 MB
                    shared: true); // Enable shared file access for multi-process scenarios
            }

            // Create and register Serilog logger
            Log.Logger = loggerConfiguration.CreateLogger();

            // Add Serilog to Microsoft.Extensions.Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            // Register correlation id services as singletons
            services.AddSingleton<ICorrelationIdGenerator, CorrelationIdGenerator>();
            services.AddSingleton<CorrelationIdMiddleware>();

            // Register structured logger as singleton
            services.AddSingleton<StructuredLogger>();

            return services;
        }

        /// <summary>
        /// Registers error handling services including exit code mapper and global exception handler.
        /// These services provide centralized error handling and scheduler-friendly exit code mapping
        /// for batch job execution failures.
        /// </summary>
        /// <param name="services">The service collection to add error handling services to.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Register error handling services as singletons
            services.AddSingleton<ExitCodeMapper>();
            services.AddSingleton<GlobalExceptionHandler>();

            return services;
        }

        /// <summary>
        /// Binds DatabaseOptions from configuration and registers as singleton.
        /// Validates database configuration at startup to ensure required connection settings are present.
        /// </summary>
        /// <param name="services">The service collection to add database configuration to.</param>
        /// <param name="configuration">The application configuration containing database settings.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when database configuration is invalid.</exception>
        public static IServiceCollection AddDatabaseConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Bind and validate DatabaseOptions from configuration
            var databaseOptions = configuration
                .GetSection(DatabaseOptions.SectionName)
                .Get<DatabaseOptions>() ?? new DatabaseOptions();

            // Validate database options
            if (!databaseOptions.IsValid())
            {
                throw new InvalidOperationException(
                    "Invalid database configuration. Please check appsettings.json Database section. " +
                    "Ensure ConnectionString is provided and all values are within valid ranges.");
            }

            // Register DatabaseOptions as singleton
            services.AddSingleton(databaseOptions);

            return services;
        }

        /// <summary>
        /// Binds JobOptions from configuration and registers as singleton.
        /// Provides job execution settings including timeout, concurrency, and retry configuration.
        /// </summary>
        /// <param name="services">The service collection to add job configuration to.</param>
        /// <param name="configuration">The application configuration containing job settings.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
        public static IServiceCollection AddJobConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Bind JobOptions from configuration
            var jobOptions = configuration
                .GetSection(JobOptions.SectionName)
                .Get<JobOptions>() ?? new JobOptions();

            // Register JobOptions as singleton
            services.AddSingleton(jobOptions);

            return services;
        }
    }
}


**Changes Made:**

1. **Added `shared: true` parameter to Serilog File sink**: This enables shared file access, which is important for batch job scenarios where multiple processes might write to the same log file. This prevents file locking issues in distributed or multi-process environments, which is common in AWS batch job architectures.

2. **Code formatting consistency**: Ensured consistent formatting throughout the file, particularly in the file sink configuration where the comment was moved to the same line as the parameter for better readability.

The code already follows .NET best practices including:
- Proper null checking with `ArgumentNullException.ThrowIfNull`
- Configuration validation at startup
- Singleton lifetime for stateless services
- Method chaining pattern for fluent API
- Comprehensive XML documentation
- Structured logging with Serilog
- Separation of concerns with dedicated extension methods