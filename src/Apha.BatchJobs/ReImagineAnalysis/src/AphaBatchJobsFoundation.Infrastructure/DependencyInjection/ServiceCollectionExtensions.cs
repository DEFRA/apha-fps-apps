// ============================================================================
// File: ServiceCollectionExtensions.cs
// Description: Extension methods for dependency injection setup of core services,
//              logging, and configuration with startup validation
// Project: AphaBatchJobsFoundation.Infrastructure
// Layer: Infrastructure - Dependency Injection
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using AphaBatchJobsFoundation.Core.Interfaces;
using AphaBatchJobsFoundation.Application.Orchestration;
using AphaBatchJobsFoundation.Infrastructure.Configuration;
using AphaBatchJobsFoundation.Infrastructure.Logging;

namespace AphaBatchJobsFoundation.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for IServiceCollection to configure Apha BatchJobs Foundation services.
    /// Includes registration of core services, logging infrastructure, and configuration options
    /// with comprehensive startup validation.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers core Apha BatchJobs Foundation services including orchestrator, logger, and configuration.
        /// This is the primary extension method that should be called during application startup.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="configuration">The application configuration containing settings</param>
        /// <returns>The service collection for method chaining</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when configuration validation fails</exception>
        public static IServiceCollection AddAphaJobFoundation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Register configuration options with validation
            services.AddAphaConfiguration(configuration);

            // Register structured logging with correlation id support
            services.AddAphaLogging(configuration);

            // Register core orchestrator service
            services.AddSingleton<IJobOrchestrator, JobOrchestrator>();

            // REMOVED: Building service provider during registration is an anti-pattern
            // ValidateOnStart() in AddAphaConfiguration already handles startup validation
            // Building service provider here can cause issues with scoped services and disposal

            return services;
        }

        /// <summary>
        /// Configures structured logging with correlation id support using Serilog.
        /// Sets up console and optional file logging based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection to add logging services to</param>
        /// <param name="configuration">The application configuration containing logging settings</param>
        /// <returns>The service collection for method chaining</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
        public static IServiceCollection AddAphaLogging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Bind logging options from configuration
            var loggingOptions = new AphaLoggingOptions();
            configuration.GetSection(AphaLoggingOptions.SectionName).Bind(loggingOptions);

            // Parse log level from configuration
            var logLevel = ParseLogLevel(loggingOptions.LogLevel);

            // Configure Serilog logger
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "AphaBatchJobsFoundation");

            // Add console logging if enabled
            if (loggingOptions.LogToConsole)
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            // Add file logging if enabled
            if (loggingOptions.LogToFile)
            {
                var logFilePath = string.IsNullOrWhiteSpace(loggingOptions.LogFilePath)
                    ? "logs/apha-batchjobs-.log"
                    : loggingOptions.LogFilePath;

                loggerConfiguration.WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingOptions.LogFileRetentionDays,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            // Create and register Serilog logger
            Log.Logger = loggerConfiguration.CreateLogger();

            // Add Serilog to Microsoft.Extensions.Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // Register AphaLogger wrapper
            services.AddSingleton<AphaLogger>();

            return services;
        }

        /// <summary>
        /// Binds and validates configuration options for Apha BatchJobs Foundation.
        /// Registers AphaJobOptions and AphaLoggingOptions with the service collection
        /// and enables data annotation validation.
        /// </summary>
        /// <param name="services">The service collection to add configuration to</param>
        /// <param name="configuration">The application configuration containing settings</param>
        /// <returns>The service collection for method chaining</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
        public static IServiceCollection AddAphaConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Register and validate AphaJobOptions with custom validation
            services.AddOptions<AphaJobOptions>()
                .Bind(configuration.GetSection(AphaJobOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(options =>
                {
                    // Custom validation for ConnectionString
                    if (string.IsNullOrWhiteSpace(options.ConnectionString))
                    {
                        return false;
                    }

                    // Custom validation for CommandTimeout
                    if (options.CommandTimeout <= 0)
                    {
                        return false;
                    }

                    // Custom validation for MaxRetryAttempts
                    if (options.MaxRetryAttempts < 0)
                    {
                        return false;
                    }

                    return true;
                }, "AphaJobOptions validation failed. Ensure ConnectionString is not empty, CommandTimeout > 0, and MaxRetryAttempts >= 0.")
                .ValidateOnStart();

            // Register and validate AphaLoggingOptions with custom validation
            services.AddOptions<AphaLoggingOptions>()
                .Bind(configuration.GetSection(AphaLoggingOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(options =>
                {
                    // Custom validation for LogLevel
                    if (string.IsNullOrWhiteSpace(options.LogLevel) || !IsValidLogLevel(options.LogLevel))
                    {
                        return false;
                    }

                    // Custom validation for LogFileRetentionDays when LogToFile is enabled
                    if (options.LogToFile && options.LogFileRetentionDays <= 0)
                    {
                        return false;
                    }

                    return true;
                }, "AphaLoggingOptions validation failed. Ensure LogLevel is valid (Trace, Debug, Information, Warning, Error, Critical, None) and LogFileRetentionDays > 0 when LogToFile is enabled.")
                .ValidateOnStart();

            // Register options as singleton for direct injection
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AphaJobOptions>>().Value);
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AphaLoggingOptions>>().Value);

            return services;
        }

        /// <summary>
        /// Parses log level string to Serilog LogEventLevel enum.
        /// Defaults to Information level if parsing fails.
        /// </summary>
        /// <param name="logLevel">The log level string to parse</param>
        /// <returns>The corresponding LogEventLevel enum value</returns>
        private static LogEventLevel ParseLogLevel(string logLevel)
        {
            if (string.IsNullOrWhiteSpace(logLevel))
            {
                return LogEventLevel.Information;
            }

            return logLevel.ToUpperInvariant() switch
            {
                "TRACE" => LogEventLevel.Verbose,
                "DEBUG" => LogEventLevel.Debug,
                "INFORMATION" => LogEventLevel.Information,
                "WARNING" => LogEventLevel.Warning,
                "ERROR" => LogEventLevel.Error,
                "CRITICAL" => LogEventLevel.Fatal,
                "NONE" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }

        /// <summary>
        /// Validates if the provided log level string is a valid log level value.
        /// </summary>
        /// <param name="logLevel">The log level string to validate</param>
        /// <returns>True if the log level is valid, false otherwise</returns>
        private static bool IsValidLogLevel(string logLevel)
        {
            if (string.IsNullOrWhiteSpace(logLevel))
            {
                return false;
            }

            var upperLogLevel = logLevel.ToUpperInvariant();
            return upperLogLevel == "TRACE" ||
                   upperLogLevel == "DEBUG" ||
                   upperLogLevel == "INFORMATION" ||
                   upperLogLevel == "WARNING" ||
                   upperLogLevel == "ERROR" ||
                   upperLogLevel == "CRITICAL" ||
                   upperLogLevel == "NONE";
        }
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Architecture Decisions:
// 1. Extension method pattern for clean DI registration following .NET conventions
// 2. Separate methods for logging, configuration, and core services for modularity
// 3. Serilog integration for structured logging with correlation id support
// 4. Options pattern with data annotation validation and startup validation
// 5. Comprehensive validation to fail fast on configuration errors
//
// Dependency Registration:
// - IJobOrchestrator -> JobOrchestrator (Singleton)
// - AphaLogger (Singleton wrapper around ILogger)
// - AphaJobOptions (Singleton from IOptions<T>)
// - AphaLoggingOptions (Singleton from IOptions<T>)
// - Serilog logger configured and added to Microsoft.Extensions.Logging
//
// Configuration Validation:
// - Data annotation validation via ValidateDataAnnotations()
// - Custom validation via Validate() method with descriptive error messages
// - Startup validation via ValidateOnStart()
// - Validates ConnectionString, CommandTimeout, MaxRetryAttempts
// - Validates LogLevel, LogFileRetentionDays
// - Throws OptionsValidationException with descriptive messages on failure
//
// KEY IMPROVEMENTS:
// 1. REMOVED ValidateConfiguration method and BuildServiceProvider call
//    - Building ServiceProvider during registration is an anti-pattern
//    - Can cause issues with scoped services and disposal
//    - ValidateOnStart() already handles startup validation properly
//
// 2. MOVED validation logic to Options pattern Validate() method
//    - More idiomatic .NET approach
//    - Better integration with IOptions<T> infrastructure
//    - Cleaner error messages through OptionsValidationException
//    - Validation happens at the right time in the DI lifecycle
//
// 3. IMPROVED validation error messages
//    - Single, clear error message per options class
//    - Easier to troubleshoot configuration issues
//
// Logging Configuration:
// - Serilog configured with console and file sinks based on options
// - Correlation id support via log context enrichment
// - Configurable log levels and output templates
// - Rolling file logs with retention policy
// - Default log file path if not specified
//
// Error Handling:
// - ArgumentNullException for null parameters
// - OptionsValidationException for configuration validation failures (via ValidateOnStart)
// - Descriptive error messages for troubleshooting
//
// Best Practices Applied:
// - Null parameter validation on all public methods
// - Method chaining support via return this
// - Private helper methods for parsing and validation
// - Comprehensive XML documentation
// - Follows Apha naming conventions
// - Clean separation of concerns
// - Proper use of Options pattern validation
// - Avoids anti-pattern of building ServiceProvider during registration
//
// Startup Sequence:
// 1. AddAphaJobFoundation called from Program.cs
// 2. Configuration options registered and bound
// 3. Logging infrastructure configured
// 4. Core services registered
// 5. Configuration validated at startup via ValidateOnStart()
// 6. Application starts or fails fast with clear error message
//
// Future Extensibility:
// - Can add database connection validation
// - Can add health check registration
// - Can add metrics and monitoring setup
// - Can add additional service registrations
// - Can add environment-specific configuration
//
// ============================================================================