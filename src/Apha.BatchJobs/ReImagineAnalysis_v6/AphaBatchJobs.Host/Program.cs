using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Infrastructure.Configuration;
using AphaBatchJobs.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Host
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            IHost? host = null;
            try
            {
                var (isScheduled, jobName, parameters) = ParseArguments(args);

                host = CreateHostBuilder(args).Build();

                // Use AsyncServiceScope for proper async disposal
                await using var scope = host.Services.CreateAsyncScope();
                var jobRunner = scope.ServiceProvider.GetRequiredService<IJobRunnerService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    logger.LogInformation("Cancellation requested. Shutting down gracefully...");
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };

                var exitCode = await ExecuteJobAsync(jobRunner, isScheduled, jobName, parameters, cts.Token);

                return exitCode;
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Operation was cancelled.");
                return 130; // Standard exit code for SIGINT
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fatal error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
            finally
            {
                // Ensure proper disposal of host resources
                if (host != null)
                {
                    await host.StopAsync(TimeSpan.FromSeconds(5));
                    host.Dispose();
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // SetBasePath is redundant with CreateDefaultBuilder
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false); // reloadOnChange should be false for batch jobs
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    config.AddEnvironmentVariables();
                    
                    // Command line args should be added last to have highest priority
                    if (args != null && args.Length > 0)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    
                    // Add AWS CloudWatch logging if running on AWS
                    // This would be configured through AddInfrastructureServices
                })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    ConfigureServices(hostBuilderContext, services);
                });

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // Register JobRunnerService with proper lifetime scope
            services.AddScoped<IJobRunnerService, AphaBatchJobs.Application.Services.JobRunnerService>();

            // Register infrastructure services (includes DB context, repositories, etc.)
            services.AddInfrastructureServices(context.Configuration);

            // Bind configuration options with validation
            services.Configure<DatabaseOptions>(context.Configuration.GetSection(DatabaseOptions.SectionName));
            services.Configure<JobOptions>(context.Configuration.GetSection("JobOptions"));
            
            // Add options validation
            services.AddOptions<DatabaseOptions>()
                .Bind(context.Configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
                
            services.AddOptions<JobOptions>()
                .Bind(context.Configuration.GetSection("JobOptions"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        private static (bool isScheduled, string jobName, Dictionary<string, string> parameters) ParseArguments(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return (false, string.Empty, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }

            bool isScheduled = false;
            string jobName = string.Empty;
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--scheduled", StringComparison.OrdinalIgnoreCase))
                {
                    isScheduled = true;
                }
                else if (args[i].Equals("--adhoc", StringComparison.OrdinalIgnoreCase))
                {
                    isScheduled = false;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    {
                        jobName = args[i + 1];
                        i++;
                    }
                }
                else if (args[i].StartsWith("--", StringComparison.Ordinal) && i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    var key = args[i][2..]; // Use range operator instead of Substring
                    var value = args[i + 1];
                    parameters[key] = value;
                    i++;
                }
            }

            return (isScheduled, jobName, parameters);
        }

        private static async Task<int> ExecuteJobAsync(
            IJobRunnerService jobRunner,
            bool isScheduled,
            string jobName,
            Dictionary<string, string> parameters,
            CancellationToken cancellationToken)
        {
            if (isScheduled)
            {
                return await jobRunner.RunScheduledJobsAsync(cancellationToken);
            }
            
            if (string.IsNullOrWhiteSpace(jobName))
            {
                Console.Error.WriteLine("Error: Job name is required for adhoc execution. Use --adhoc <jobName>");
                return 1;
            }

            // Direct cast is safe here since Dictionary<string, string> implements IReadOnlyDictionary<string, string>
            IReadOnlyDictionary<string, string> readOnlyParameters = parameters;
            return await jobRunner.RunAdhocJobAsync(jobName, readOnlyParameters, cancellationToken);
        }
    }
}


**Key improvements made:**

1. **Removed unused `using` statements** - Removed `System.Linq` which wasn't being used
2. **Proper async disposal** - Changed to `await using` for `AsyncServiceScope` for proper async resource cleanup
3. **Added logger usage** - Retrieved logger from DI container for better logging in cancellation handler
4. **Proper host disposal** - Added `finally` block to ensure host is properly stopped and disposed
5. **OperationCanceledException handling** - Added specific catch for cancellation with appropriate exit code (130)
6. **Disabled reloadOnChange for batch jobs** - Set to `false` as file watching is unnecessary overhead for batch jobs
7. **Added null check for args** - Added safety check in `CreateHostBuilder` and `ParseArguments`
8. **Options validation** - Added `ValidateDataAnnotations()` and `ValidateOnStart()` for configuration validation
9. **Range operator** - Used modern C# range operator `[2..]` instead of `Substring(2)`
10. **Simplified conditional logic** - Removed unnecessary null-coalescing in `ExecuteJobAsync`
11. **StringComparison.Ordinal** - Used `Ordinal` instead of default for `StartsWith` checks (better performance)
12. **Graceful shutdown timeout** - Added 5-second timeout for `StopAsync` to ensure graceful shutdown