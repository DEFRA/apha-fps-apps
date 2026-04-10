using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Infrastructure.Extensions;
using AphaBatchJobs.Infrastructure.Utilities;

namespace AphaBatchJobs.Host;

/// <summary>
/// Entry point for the AphaBatchJobs application.
/// Orchestrates job execution based on command line arguments for scheduled and adhoc batch operations.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point that creates the host, configures services, and orchestrates job execution.
    /// </summary>
    /// <param name="args">Command line arguments to determine execution mode (--scheduled or --adhoc).</param>
    /// <returns>Exit code indicating the result of the operation.</returns>
    public static async Task<int> Main(string[] args)
    {
        IHost? host = null;
        ILogger<Program>? logger = null;

        try
        {
            // Create and configure the Generic Host using Host.CreateDefaultBuilder
            host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register all infrastructure services including database context and job services
                    services.AddBatchJobsInfrastructure(hostContext.Configuration);
                })
                .Build();

            // Resolve logger for exception handling and diagnostics
            logger = host.Services.GetRequiredService<ILogger<Program>>();

            // Parse command line arguments to determine execution mode
            if (args == null || args.Length == 0)
            {
                logger.LogError("No command line arguments provided. Expected --scheduled or --adhoc <jobName>");
                return 1;
            }

            // Determine trigger mode based on first argument
            string triggerMode = args[0].ToLowerInvariant();

            // Resolve the job runner service from the dependency injection container
            // Use AsyncServiceScope for proper async disposal
            await using var scope = host.Services.CreateAsyncScope();
            var jobRunnerService = scope.ServiceProvider.GetRequiredService<IJobRunnerService>();

            int exitCode;

            if (triggerMode == "--scheduled")
            {
                // Execute all scheduled jobs
                logger.LogInformation("Starting scheduled job execution");
                exitCode = await jobRunnerService.RunScheduledAsync(CancellationToken.None);
                logger.LogInformation("Scheduled job execution completed with exit code: {ExitCode}", exitCode);
            }
            else if (triggerMode == "--adhoc")
            {
                // Execute specific adhoc job by name
                if (args.Length < 2)
                {
                    logger.LogError("Adhoc mode requires a job name parameter. Usage: --adhoc <jobName>");
                    return 1;
                }

                string jobName = args[1];
                logger.LogInformation("Starting adhoc job execution for job: {JobName}", jobName);
                exitCode = await jobRunnerService.RunAdhocAsync(jobName, CancellationToken.None);
                logger.LogInformation("Adhoc job execution completed with exit code: {ExitCode}", exitCode);
            }
            else
            {
                logger.LogError("Invalid trigger mode: {TriggerMode}. Expected --scheduled or --adhoc", triggerMode);
                return 1;
            }

            return exitCode;
        }
        catch (Exception ex)
        {
            // Handle all unhandled exceptions using GlobalExceptionHandler
            if (logger != null)
            {
                GlobalExceptionHandler.Handle(ex, logger);
                int exitCode = ExitCodeMapper.Map(ex);
                logger.LogCritical("Application terminated with exit code: {ExitCode}", exitCode);
                // Remove Environment.Exit() to allow proper cleanup in finally block
                return exitCode;
            }
            else
            {
                // Fallback if logger is not available (early initialization failure)
                Console.Error.WriteLine($"Fatal error during initialization: {ex.Message}");
                int exitCode = ExitCodeMapper.Map(ex);
                // Remove Environment.Exit() to allow proper cleanup in finally block
                return exitCode;
            }
        }
        finally
        {
            // Ensure proper disposal of host resources
            if (host != null)
            {
                // Use CancellationToken for graceful shutdown
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try
                {
                    await host.StopAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Log if available, otherwise ignore timeout during shutdown
                    logger?.LogWarning("Host shutdown timed out after 5 seconds");
                }
                finally
                {
                    // Dispose host asynchronously if possible
                    if (host is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    else
                    {
                        host.Dispose();
                    }
                }
            }
        }
    }
}


**Key improvements made:**

1. **AsyncServiceScope**: Changed `using var scope` to `await using var scope` with `CreateAsyncScope()` for proper async disposal of scoped services, especially important for database connections.

2. **Removed Environment.Exit()**: Removed `Environment.Exit()` calls in catch blocks to allow the finally block to execute properly and ensure graceful cleanup of resources.

3. **Removed unused variable assignment**: Removed `var result =` from `GlobalExceptionHandler.Handle()` call since the result was not being used.

4. **Improved shutdown handling**: Added CancellationToken to `StopAsync()` with proper timeout handling and exception catching for graceful shutdown.

5. **Async disposal of host**: Added check for `IAsyncDisposable` to properly dispose the host asynchronously, which is important for PostgreSQL connection cleanup.

6. **Better exception handling during shutdown**: Wrapped shutdown logic in try-catch-finally to handle timeout scenarios gracefully.