using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Infrastructure.DependencyInjection;
using AphaBatchJobs.Infrastructure.ErrorHandling;
using AphaBatchJobs.Infrastructure.Validation;

namespace AphaBatchJobs.Host;

/// <summary>
/// Entry point class for the AphaBatchJobs application.
/// Configures the host, dependency injection, and orchestrates job execution based on CLI arguments.
/// Supports two execution modes:
/// 1. Scheduled mode: --scheduled flag triggers all registered scheduled jobs
/// 2. Adhoc mode: --adhoc [JobName] flag triggers a specific named job
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point for the AphaBatchJobs application.
    /// Configures the Generic Host, validates configuration, parses command-line arguments,
    /// and executes the appropriate job runner based on the trigger mode.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments. Expected formats:
    /// - For scheduled jobs: --scheduled
    /// - For adhoc jobs: --adhoc [JobName]
    /// </param>
    /// <returns>
    /// Exit code as integer:
    /// 0 = Success
    /// 1 = General error
    /// 2 = Database error
    /// 3 = Configuration error
    /// </returns>
    public static async Task<int> Main(string[] args)
    {
        IHost? host = null;
        ILogger<Program>? logger = null;
        var exitCode = 0;

        try
        {
            // Build the Generic Host with configuration and dependency injection
            host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Clear default configuration sources to have explicit control
                    config.Sources.Clear();

                    // Add appsettings.json as the base configuration
                    config.AddJsonFile(
                        path: "appsettings.json",
                        optional: false,
                        reloadOnChange: false);

                    // Add environment-specific configuration (appsettings.Development.json, etc.)
                    var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                    config.AddJsonFile(
                        path: $"appsettings.{environmentName}.json",
                        optional: true,
                        reloadOnChange: false);

                    // Add environment variables for ECS Fargate configuration override
                    config.AddEnvironmentVariables();

                    // Add command-line arguments for runtime configuration
                    if (args is { Length: > 0 })
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Register all batch job infrastructure and application services
                    services.AddBatchJobsInfrastructure(hostContext.Configuration);
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    // Configure logging for ECS Fargate CloudWatch integration
                    logging.ClearProviders();
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .Build();

            // Resolve logger for error handling and operational logging
            logger = host.Services.GetRequiredService<ILogger<Program>>();

            // Get configuration for validation
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            // Validate critical configuration before proceeding
            ConfigurationValidator.Validate(configuration);

            logger.LogInformation("AphaBatchJobs host started successfully");
            logger.LogInformation("Command-line arguments: {Args}", string.Join(" ", args ?? Array.Empty<string>()));

            // Parse command-line arguments to determine execution mode
            if (args is not { Length: > 0 })
            {
                const string errorMessage = "No command-line arguments provided. Use --scheduled for scheduled jobs or --adhoc [JobName] for adhoc jobs.";
                logger.LogError("No command-line arguments provided. Expected --scheduled or --adhoc [JobName]");
                throw new InvalidOperationException(errorMessage);
            }

            // Resolve the job runner service from dependency injection
            using var scope = host.Services.CreateScope();
            var jobRunnerService = scope.ServiceProvider.GetRequiredService<IJobRunnerService>();

            // Execute based on trigger mode
            if (args.Contains("--scheduled", StringComparer.OrdinalIgnoreCase))
            {
                logger.LogInformation("Executing scheduled job mode");
                exitCode = await jobRunnerService.RunScheduledAsync(CancellationToken.None);
                logger.LogInformation("Scheduled job execution completed with exit code {ExitCode}", exitCode);
            }
            else if (args.Contains("--adhoc", StringComparer.OrdinalIgnoreCase))
            {
                // Find the job name parameter (next argument after --adhoc)
                var adhocIndex = Array.FindIndex(args, arg => 
                    string.Equals(arg, "--adhoc", StringComparison.OrdinalIgnoreCase));

                if (adhocIndex == -1 || adhocIndex + 1 >= args.Length)
                {
                    const string errorMessage = "Job name is required when using --adhoc flag. Usage: --adhoc [JobName]";
                    logger.LogError("--adhoc flag provided but job name is missing");
                    throw new InvalidOperationException(errorMessage);
                }

                var jobName = args[adhocIndex + 1];

                if (string.IsNullOrWhiteSpace(jobName))
                {
                    const string errorMessage = "Job name cannot be empty. Usage: --adhoc [JobName]";
                    logger.LogError("--adhoc flag provided but job name is empty");
                    throw new InvalidOperationException(errorMessage);
                }

                logger.LogInformation("Executing adhoc job mode for job: {JobName}", jobName);
                exitCode = await jobRunnerService.RunAdhocAsync(jobName, CancellationToken.None);
                logger.LogInformation("Adhoc job {JobName} execution completed with exit code {ExitCode}", 
                    jobName, exitCode);
            }
            else
            {
                const string errorMessage = "Invalid command-line arguments. Use --scheduled for scheduled jobs or --adhoc [JobName] for adhoc jobs.";
                logger.LogError("Invalid command-line arguments. Expected --scheduled or --adhoc [JobName]");
                throw new InvalidOperationException(errorMessage);
            }
        }
        catch (Exception ex)
        {
            // Handle all exceptions through the global exception handler
            if (logger is not null)
            {
                var result = GlobalExceptionHandler.Handle(ex, logger);
                exitCode = result.ExitCode;
                logger.LogError("Application terminated with exit code {ExitCode}", exitCode);
            }
            else
            {
                // Fallback logging if logger is not available
                Console.Error.WriteLine($"Fatal error during application startup: {ex.Message}");
                Console.Error.WriteLine($"Exception type: {ex.GetType().FullName}");
                exitCode = ExitCodeMapper.Map(ex);
            }
        }
        finally
        {
            // Ensure proper cleanup of host resources
            if (host is not null)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
                host.Dispose();
            }

            logger?.LogInformation("AphaBatchJobs host shutdown complete");
        }

        // Exit the process with the appropriate exit code for ECS Fargate task status
        Environment.Exit(exitCode);
        return exitCode;
    }
}


// Key improvements made:
// 1. Used pattern matching with 'is' for null checks (args is { Length: > 0 }) - more idiomatic C# 10
// 2. Used 'is not null' pattern instead of '!= null' for consistency with modern C# style
// 3. Extracted duplicate error messages into const strings to avoid repetition and improve maintainability
// 4. Used null-conditional operator (?.) for logger in finally block for cleaner code
// 5. Changed 'int exitCode = 0' to 'var exitCode = 0' for consistency with type inference best practices
// 6. Maintained all existing functionality without adding new features
// 7. Preserved all logging, error handling, and ECS Fargate-specific configurations
// 8. Kept the explicit Environment.Exit() call which is appropriate for containerized batch jobs