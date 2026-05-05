using System;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundationV3.Core.Enums;
using AphaBatchJobsFoundationV3.Host.Configuration;
using AphaBatchJobsFoundationV3.Host.Extensions;
using AphaBatchJobsFoundationV3.Host.Services;
using AphaBatchJobsFoundationV3.Infrastructure.Extensions;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AphaBatchJobsFoundationV3.Host
{
    /// <summary>
    /// Application entry point for Apha Batch Jobs Foundation.
    /// Configures and runs the host with support for both scheduler and CLI execution modes.
    /// Implements structured logging with correlation ID, dependency injection, and exit code mapping.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// Parses command-line arguments, configures services with DI extensions,
        /// sets up Serilog logging with correlation ID, handles both scheduler and CLI execution modes
        /// with exit code mapping and global exception handling.
        /// </summary>
        /// <param name="args">Command-line arguments for execution mode and job configuration.</param>
        /// <returns>Exit code as integer indicating execution outcome.</returns>
        public static async Task<int> Main(string[] args)
        {
            // Configure bootstrap logger for early initialization logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Apha Batch Jobs Foundation Host");

                // Parse and validate command-line arguments
                var options = ParseCommandLineArguments(args);
                if (options is null)
                {
                    return (int)ExitCode.ValidationError;
                }

                // Validate execution mode
                var validationResult = ValidateOptions(options);
                if (validationResult != ExitCode.Success)
                {
                    return (int)validationResult;
                }

                LogExecutionMode(options);

                // Create and configure host builder
                var hostBuilder = CreateHostBuilder(args);
                var host = hostBuilder.Build();

                // Execute based on mode
                return await ExecuteAsync(host, options).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Global exception handling
                Log.Fatal(ex, "Unhandled exception occurred during application execution");
                return (int)ExitCode.UnhandledException;
            }
            finally
            {
                // Ensure all logs are flushed before application exit
                await Log.CloseAndFlushAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Parses command-line arguments and returns the parsed options.
        /// </summary>
        /// <param name="args">Command-line arguments to parse.</param>
        /// <returns>Parsed CommandLineOptions or null if parsing failed.</returns>
        private static CommandLineOptions? ParseCommandLineArguments(string[] args)
        {
            CommandLineOptions? options = null;
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(parsed => options = parsed)
                .WithNotParsed(errors =>
                {
                    Log.Error("Failed to parse command-line arguments");
                    foreach (var error in errors)
                    {
                        Log.Error("Argument error: {ErrorTag}", error.Tag);
                    }
                });

            if (options is null)
            {
                Log.Error("Command-line argument parsing failed");
            }

            return options;
        }

        /// <summary>
        /// Validates the parsed command-line options.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <returns>ExitCode indicating validation result.</returns>
        private static ExitCode ValidateOptions(CommandLineOptions options)
        {
            // Validate mode
            if (options.Mode != CommandLineOptions.SchedulerMode && 
                options.Mode != CommandLineOptions.CliMode)
            {
                Log.Error("Invalid execution mode: {Mode}. Valid modes are: {SchedulerMode} or {CliMode}", 
                    options.Mode, CommandLineOptions.SchedulerMode, CommandLineOptions.CliMode);
                return ExitCode.ValidationError;
            }

            // Validate CLI mode requirements
            if (options.Mode == CommandLineOptions.CliMode && string.IsNullOrWhiteSpace(options.JobName))
            {
                Log.Error("Job name is required when running in CLI mode");
                return ExitCode.ValidationError;
            }

            return ExitCode.Success;
        }

        /// <summary>
        /// Logs the execution mode and job name if applicable.
        /// </summary>
        /// <param name="options">The command-line options.</param>
        private static void LogExecutionMode(CommandLineOptions options)
        {
            Log.Information("Execution mode: {Mode}", options.Mode);
            if (options.Mode == CommandLineOptions.CliMode)
            {
                Log.Information("Job name: {JobName}", options.JobName);
            }
        }

        /// <summary>
        /// Executes the application based on the specified mode.
        /// </summary>
        /// <param name="host">The configured host.</param>
        /// <param name="options">The command-line options.</param>
        /// <returns>Exit code indicating execution outcome.</returns>
        private static async Task<int> ExecuteAsync(IHost host, CommandLineOptions options)
        {
            if (options.Mode == CommandLineOptions.SchedulerMode)
            {
                Log.Information("Running in scheduler mode");
                await host.RunAsync().ConfigureAwait(false);
                return (int)ExitCode.Success;
            }
            
            // CLI mode
            Log.Information("Running in CLI mode");
            var cliJobExecutor = host.Services.GetRequiredService<CliJobExecutor>();
            
            // Execute job and get exit code
            var exitCode = await cliJobExecutor.ExecuteJobAsync(
                options.JobName!, 
                CancellationToken.None).ConfigureAwait(false);
            
            Log.Information("CLI job execution completed with exit code: {ExitCode}", exitCode);
            return (int)exitCode;
        }

        /// <summary>
        /// Creates and configures the host builder with all required services and configuration.
        /// Configures appsettings.json, appsettings.Development.json, structured logging,
        /// infrastructure services, and host services.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        /// <returns>Configured IHostBuilder instance.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Clear default configuration sources to have explicit control
                    config.Sources.Clear();

                    // Add configuration sources in order of precedence
                    config
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                            optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .AddStructuredLogging()
                .ConfigureServices((context, services) =>
                {
                    // Register infrastructure services (DbContext, CorrelationService, Quartz)
                    services.AddInfrastructureServices(context.Configuration);

                    // Register host services (CliJobExecutor, SchedulerHostedService)
                    services.AddHostServices();
                });
        }
    }
}


// Key improvements made:
// 1. Extracted ParseCommandLineArguments method to improve separation of concerns and testability
// 2. Extracted ValidateOptions method to consolidate validation logic and return ExitCode enum instead of int
// 3. Extracted LogExecutionMode method to separate logging concerns
// 4. Extracted ExecuteAsync method to handle execution logic, reducing complexity in Main method
// 5. Removed redundant host.Build() call by consolidating it once before ExecuteAsync
// 6. Improved code readability by breaking down the Main method into smaller, focused methods
// 7. Maintained all existing functionality without adding new features
// 8. Preserved all ConfigureAwait(false) calls for proper async/await behavior
// 9. Kept all XML documentation comments intact
