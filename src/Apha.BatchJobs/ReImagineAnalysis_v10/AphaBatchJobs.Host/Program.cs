using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Infrastructure.Extensions;
using AphaBatchJobs.Infrastructure.Utilities;

namespace AphaBatchJobs.Host;

/// <summary>
/// Entry point for the AphaBatchJobs application.
/// Supports two execution modes:
/// - Scheduled: Runs all registered scheduled jobs (--scheduled flag)
/// - Adhoc: Runs a specific job by name (--adhoc [jobName] flags)
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// Creates a Generic Host, configures services, parses command line arguments,
    /// executes the appropriate job runner method, and exits with an appropriate exit code.
    /// </summary>
    /// <param name="args">Command line arguments. Expected formats:
    /// - For scheduled jobs: --scheduled
    /// - For adhoc jobs: --adhoc [jobName]
    /// </param>
    /// <returns>Exit code indicating execution status.</returns>
    public static async Task<int> Main(string[] args)
    {
        IHost? host = null;
        ILogger<Program>? logger = null;
        int exitCode = 1;

        try
        {
            // Create and configure the Generic Host using Host.CreateDefaultBuilder
            host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register all batch jobs infrastructure services
                    services.AddBatchJobsInfrastructure(hostContext.Configuration);
                })
                .Build();

            // Resolve logger for application-level logging
            logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("AphaBatchJobs application started. Arguments: {Arguments}", string.Join(" ", args));

            // Parse command line arguments to determine execution mode
            var executionMode = ParseExecutionMode(args, out string? jobName);

            if (executionMode == ExecutionMode.Unknown)
            {
                logger.LogError("Invalid command line arguments. Expected --scheduled or --adhoc [jobName]");
                return 1;
            }

            // Create a service scope for job execution
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var jobRunnerService = scope.ServiceProvider.GetRequiredService<IJobRunnerService>();
                using var cancellationTokenSource = new CancellationTokenSource();

                // Handle Ctrl+C gracefully
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    logger.LogWarning("Cancellation requested. Attempting graceful shutdown...");
                    cancellationTokenSource.Cancel();
                    eventArgs.Cancel = true;
                };

                // Execute the appropriate job runner method based on execution mode
                if (executionMode == ExecutionMode.Scheduled)
                {
                    logger.LogInformation("Executing scheduled jobs...");
                    exitCode = await jobRunnerService.RunScheduledAsync(cancellationTokenSource.Token);
                    logger.LogInformation("Scheduled jobs completed with exit code: {ExitCode}", exitCode);
                }
                else if (executionMode == ExecutionMode.Adhoc)
                {
                    if (string.IsNullOrWhiteSpace(jobName))
                    {
                        logger.LogError("Job name is required for adhoc execution");
                        return 1;
                    }

                    logger.LogInformation("Executing adhoc job: {JobName}", jobName);
                    exitCode = await jobRunnerService.RunAdhocAsync(jobName, cancellationTokenSource.Token);
                    logger.LogInformation("Adhoc job {JobName} completed with exit code: {ExitCode}", jobName, exitCode);
                }
            }

            return exitCode;
        }
        catch (Exception ex)
        {
            // Handle all unhandled exceptions using GlobalExceptionHandler
            if (logger != null)
            {
                var result = GlobalExceptionHandler.Handle(ex, logger);
                exitCode = result.ExitCode;
                logger.LogCritical("Application terminated with exit code: {ExitCode}", exitCode);
            }
            else
            {
                // Fallback if logger is not available
                Console.Error.WriteLine($"Fatal error: {ex.Message}");
                exitCode = ExitCodeMapper.Map(ex);
            }

            return exitCode;
        }
        finally
        {
            // Dispose the host if it was created
            if (host != null)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
                host.Dispose();
            }

            // Exit the application with the determined exit code
            Environment.Exit(exitCode);
        }
    }

    /// <summary>
    /// Parses command line arguments to determine the execution mode and extract job name if applicable.
    /// </summary>
    /// <param name="args">Command line arguments array.</param>
    /// <param name="jobName">Output parameter containing the job name for adhoc mode, or null otherwise.</param>
    /// <returns>The determined execution mode.</returns>
    private static ExecutionMode ParseExecutionMode(string[] args, out string? jobName)
    {
        jobName = null;

        if (args == null || args.Length == 0)
        {
            return ExecutionMode.Unknown;
        }

        // Check for --scheduled flag
        if (args.Contains("--scheduled", StringComparer.OrdinalIgnoreCase))
        {
            return ExecutionMode.Scheduled;
        }

        // Check for --adhoc flag and extract job name
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--adhoc", StringComparison.OrdinalIgnoreCase))
            {
                // Job name should be the next argument
                if (i + 1 < args.Length && !string.IsNullOrWhiteSpace(args[i + 1]))
                {
                    jobName = args[i + 1];
                    return ExecutionMode.Adhoc;
                }
                else
                {
                    // --adhoc flag found but no job name provided
                    return ExecutionMode.Unknown;
                }
            }
        }

        return ExecutionMode.Unknown;
    }

    /// <summary>
    /// Enumeration representing the execution mode of the application.
    /// </summary>
    private enum ExecutionMode
    {
        /// <summary>
        /// Unknown or invalid execution mode.
        /// </summary>
        Unknown,

        /// <summary>
        /// Scheduled jobs execution mode.
        /// </summary>
        Scheduled,

        /// <summary>
        /// Adhoc job execution mode.
        /// </summary>
        Adhoc
    }
}
