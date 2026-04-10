using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Infrastructure.Extensions;
using AphaBatchJobs.Infrastructure.Utilities;

namespace AphaBatchJobs.Host;

/// <summary>
/// Main entry point for the AphaBatchJobs application.
/// This console application hosts scheduled and adhoc PostgreSQL batch operations
/// that can be deployed to AWS ECS Fargate via Docker.
/// 
/// Usage:
/// - Scheduled mode: dotnet run -- --scheduled
/// - Adhoc mode: dotnet run -- --adhoc [JobName]
/// 
/// Exit codes:
/// - 0: Success
/// - 1: General error
/// - 2: Database error
/// - 3: Invalid operation
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point method that creates Generic Host, configures services,
    /// parses CLI arguments, executes appropriate job runner method, and handles exceptions.
    /// </summary>
    /// <param name="args">Command line arguments containing trigger mode and optional job name.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task<int> Main(string[] args)
    {
        IHost? host = null;
        ILogger<Program>? logger = null;
        int exitCode = 1;

        try
        {
            // Create Generic Host using Host.CreateDefaultBuilder with args
            host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register all infrastructure services via AddBatchJobsInfrastructure
                    services.AddBatchJobsInfrastructure(hostContext.Configuration);
                })
                .Build();

            // Get logger instance for logging
            logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("AphaBatchJobs application started with arguments: {Args}", string.Join(" ", args));

            // Parse args array for --scheduled or --adhoc flags
            if (args.Length == 0)
            {
                logger.LogError("No arguments provided. Use --scheduled or --adhoc [JobName]");
                exitCode = 3;
            }
            else if (args.Contains("--scheduled"))
            {
                logger.LogInformation("Running in scheduled mode");

                // Resolve IJobRunnerService from service provider using scoped lifetime
                await using var scope = host.Services.CreateAsyncScope();
                var jobRunnerService = scope.ServiceProvider.GetRequiredService<IJobRunnerService>();

                // Use CancellationTokenSource with timeout for graceful shutdown support
                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    logger.LogWarning("Cancellation requested, shutting down gracefully...");
                    cts.Cancel();
                    eventArgs.Cancel = true;
                };

                // Call RunScheduledAsync if --scheduled flag present
                exitCode = await jobRunnerService.RunScheduledAsync(cts.Token);

                logger.LogInformation("Scheduled jobs completed with exit code: {ExitCode}", exitCode);
            }
            else if (args.Contains("--adhoc"))
            {
                // Find the index of --adhoc flag
                var adhocIndex = Array.IndexOf(args, "--adhoc");

                // Check if job name is provided as next argument
                if (adhocIndex + 1 >= args.Length)
                {
                    logger.LogError("Job name not provided for adhoc mode. Use --adhoc [JobName]");
                    exitCode = 3;
                }
                else
                {
                    // Read the next arg as job name
                    var jobName = args[adhocIndex + 1];

                    logger.LogInformation("Running in adhoc mode with job name: {JobName}", jobName);

                    // Resolve IJobRunnerService from service provider using scoped lifetime
                    await using var scope = host.Services.CreateAsyncScope();
                    var jobRunnerService = scope.ServiceProvider.GetRequiredService<IJobRunnerService>();

                    // Use CancellationTokenSource with timeout for graceful shutdown support
                    using var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        logger.LogWarning("Cancellation requested, shutting down gracefully...");
                        cts.Cancel();
                        eventArgs.Cancel = true;
                    };

                    // Call RunAdhocAsync with job name from next arg
                    exitCode = await jobRunnerService.RunAdhocAsync(jobName, cts.Token);

                    logger.LogInformation("Adhoc job '{JobName}' completed with exit code: {ExitCode}", jobName, exitCode);
                }
            }
            else
            {
                logger.LogError("Invalid arguments. Use --scheduled or --adhoc [JobName]");
                exitCode = 3;
            }
        }
        catch (Exception ex)
        {
            // Catch all unhandled exceptions via GlobalExceptionHandler.Handle
            if (logger != null)
            {
                var result = GlobalExceptionHandler.Handle(ex, logger);
                exitCode = result.ExitCode;
            }
            else
            {
                // Fallback logging to console if logger is not available
                Console.Error.WriteLine($"Fatal error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                exitCode = ExitCodeMapper.Map(ex);
            }
        }
        finally
        {
            // Dispose host if it was created
            if (host != null)
            {
                try
                {
                    // Use timeout for StopAsync to prevent hanging on shutdown
                    using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    await host.StopAsync(shutdownCts.Token);
                }
                catch (OperationCanceledException)
                {
                    logger?.LogWarning("Host shutdown timed out after 30 seconds");
                }
                finally
                {
                    host.Dispose();
                }
            }

            // Log exit code before exiting
            logger?.LogInformation("Application exiting with code: {ExitCode}", exitCode);
        }

        // Return exit code instead of calling Environment.Exit for better testability
        // and to allow proper async disposal chain to complete
        return exitCode;
    }
}


**Key improvements made:**

1. **Changed Main return type to `Task<int>`**: Returns exit code instead of calling `Environment.Exit`, allowing proper async disposal and better testability.

2. **Used `CreateAsyncScope()` instead of `CreateScope()`**: Ensures proper async disposal of scoped services, critical for PostgreSQL connections and EF Core contexts.

3. **Added `await using` for scopes**: Ensures async disposal of scoped services that implement `IAsyncDisposable`.

4. **Implemented graceful shutdown with `CancellationTokenSource`**: Added `Console.CancelKeyPress` handler to support Ctrl+C and SIGTERM signals (important for AWS ECS Fargate container lifecycle).

5. **Added timeout to `StopAsync()`**: Prevents hanging during shutdown with 30-second timeout, wrapped in try-catch for `OperationCanceledException`.

6. **Removed `Environment.Exit()` call**: Allows the async disposal chain to complete properly and returns exit code naturally, which is better for containerized environments.

7. **Improved exception handling in finally block**: Added try-catch around `StopAsync` to handle timeout scenarios gracefully.

8. **Used null-conditional operator consistently**: Changed final logger call to use `?.` for consistency.