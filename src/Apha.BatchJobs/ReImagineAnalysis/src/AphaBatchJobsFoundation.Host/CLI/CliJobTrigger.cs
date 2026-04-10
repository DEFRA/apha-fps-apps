// ============================================================================
// File: CliJobTrigger.cs
// Description: CLI command handler for manual job triggering with argument 
//              parsing, validation, and orchestrator invocation
// Project: AphaBatchJobsFoundation.Host
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundation.Core.Enums;
using AphaBatchJobsFoundation.Core.Interfaces;
using AphaBatchJobsFoundation.Infrastructure.ErrorHandling;
using AphaBatchJobsFoundation.Infrastructure.Logging;

namespace AphaBatchJobsFoundation.Host.CLI
{
    /// <summary>
    /// Handles CLI command execution for manual job triggering.
    /// Parses command line arguments, validates input, and invokes the job orchestrator
    /// to execute scheduled or adhoc jobs with appropriate parameters.
    /// Returns scheduler-friendly exit codes for integration with external systems.
    /// </summary>
    public sealed class CliJobTrigger
    {
        private readonly IJobOrchestrator _jobOrchestrator;
        private readonly AphaLogger _logger;
        private const string CorrelationIdPrefix = "CLI";

        /// <summary>
        /// Initializes a new instance of the CliJobTrigger class.
        /// </summary>
        /// <param name="jobOrchestrator">The job orchestrator for executing jobs</param>
        /// <param name="logger">The Apha logger for structured logging with correlation id support</param>
        /// <exception cref="ArgumentNullException">Thrown when jobOrchestrator or logger is null</exception>
        public CliJobTrigger(IJobOrchestrator jobOrchestrator, AphaLogger logger)
        {
            _jobOrchestrator = jobOrchestrator ?? throw new ArgumentNullException(nameof(jobOrchestrator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the CLI job trigger asynchronously by parsing arguments, validating input,
        /// and invoking the appropriate orchestrator method based on job type.
        /// </summary>
        /// <param name="args">Command line arguments containing job name, type, and optional parameters</param>
        /// <param name="cancellationToken">Cancellation token for graceful shutdown support</param>
        /// <returns>
        /// Exit code indicating execution result:
        /// 0 = Success, 1 = General error, 2 = Configuration error, 4 = Job not found
        /// </returns>
        public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
        {
            var correlationId = $"{CorrelationIdPrefix}-{Guid.NewGuid():N}";

            try
            {
                _logger.LogInformation(correlationId, "CLI job trigger started with {ArgumentCount} arguments", args?.Length ?? 0);

                // Display usage if no arguments provided
                if (args == null || args.Length == 0)
                {
                    PrintUsage();
                    return ExitCodes.ConfigurationError;
                }

                // Parse command line arguments
                var (jobName, jobType, parameters) = ParseArguments(args);

                // Validate parsed arguments
                ValidateArguments(jobName, jobType);

                _logger.LogInformation(correlationId, "Executing job: {JobName} of type: {JobType}", jobName, jobType);

                // Execute job based on type
                var result = jobType == JobType.Scheduled
                    ? await _jobOrchestrator.ExecuteScheduledJobAsync(jobName, cancellationToken).ConfigureAwait(false)
                    : await _jobOrchestrator.ExecuteAdhocJobAsync(jobName, parameters, cancellationToken).ConfigureAwait(false);

                // Log execution result
                _logger.LogJobComplete(correlationId, jobName, result.Status, result.ExitCode);

                return result.ExitCode;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(correlationId, ex, "Invalid CLI arguments: {ErrorMessage}", ex.Message);
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine();
                PrintUsage();
                return ExitCodes.ConfigurationError;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(correlationId, "CLI job execution was cancelled");
                Console.Error.WriteLine("Operation cancelled");
                return ExitCodes.GeneralError;
            }
            catch (Exception ex)
            {
                _logger.LogError(correlationId, ex, "Unexpected error during CLI job execution: {ErrorMessage}", ex.Message);
                Console.Error.WriteLine($"Fatal error: {ex.Message}");
                return ExitCodes.GeneralError;
            }
        }

        /// <summary>
        /// Parses command line arguments to extract job name, job type, and optional parameters.
        /// Expected format: --job-name [name] --job-type [Scheduled|Adhoc] [--param key=value ...]
        /// </summary>
        /// <param name="args">Command line arguments array</param>
        /// <returns>
        /// Tuple containing:
        /// - jobName: The name of the job to execute
        /// - jobType: The type of job (Scheduled or Adhoc)
        /// - parameters: Dictionary of job parameters (empty for scheduled jobs)
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when required arguments are missing or invalid</exception>
        private static (string jobName, JobType jobType, Dictionary<string, object> parameters) ParseArguments(string[] args)
        {
            string jobName = null;
            JobType jobType = JobType.Scheduled;
            var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                switch (arg.ToLowerInvariant())
                {
                    case "--job-name":
                    case "-j":
                        if (i + 1 < args.Length)
                        {
                            jobName = args[++i];
                        }
                        else
                        {
                            throw new ArgumentException("Missing value for --job-name argument");
                        }
                        break;

                    case "--job-type":
                    case "-t":
                        if (i + 1 < args.Length)
                        {
                            var typeValue = args[++i];
                            if (!Enum.TryParse<JobType>(typeValue, ignoreCase: true, out jobType))
                            {
                                throw new ArgumentException($"Invalid job type: {typeValue}. Valid values are: Scheduled, Adhoc");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Missing value for --job-type argument");
                        }
                        break;

                    case "--param":
                    case "-p":
                        if (i + 1 < args.Length)
                        {
                            var paramValue = args[++i];
                            var parts = paramValue.Split(new[] { '=' }, 2, StringSplitOptions.None);
                            if (parts.Length == 2)
                            {
                                var key = parts[0].Trim();
                                var value = parts[1].Trim();
                                
                                if (string.IsNullOrWhiteSpace(key))
                                {
                                    throw new ArgumentException($"Invalid parameter format: {paramValue}. Parameter key cannot be empty");
                                }
                                
                                parameters[key] = value;
                            }
                            else
                            {
                                throw new ArgumentException($"Invalid parameter format: {paramValue}. Expected format: key=value");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Missing value for --param argument");
                        }
                        break;

                    case "--help":
                    case "-h":
                    case "-?":
                        PrintUsage();
                        throw new ArgumentException("Help requested");

                    default:
                        throw new ArgumentException($"Unknown argument: {arg}");
                }
            }

            return (jobName, jobType, parameters);
        }

        /// <summary>
        /// Validates that required CLI arguments are present and valid.
        /// </summary>
        /// <param name="jobName">The job name to validate</param>
        /// <param name="jobType">The job type to validate</param>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        private static void ValidateArguments(string jobName, JobType jobType)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                throw new ArgumentException("Job name is required. Use --job-name to specify the job to execute.");
            }

            if (!Enum.IsDefined(typeof(JobType), jobType))
            {
                throw new ArgumentException($"Invalid job type: {jobType}. Valid values are: Scheduled, Adhoc");
            }
        }

        /// <summary>
        /// Displays CLI usage instructions and examples to the console.
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Apha BatchJobs Foundation - CLI Job Trigger");
            Console.WriteLine("============================================");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  AphaBatchJobsFoundation.Host --job-name <name> --job-type <type> [options]");
            Console.WriteLine();
            Console.WriteLine("Required Arguments:");
            Console.WriteLine("  --job-name, -j <name>        Name of the job to execute");
            Console.WriteLine("  --job-type, -t <type>        Type of job: Scheduled or Adhoc");
            Console.WriteLine();
            Console.WriteLine("Optional Arguments:");
            Console.WriteLine("  --param, -p <key=value>      Job parameter (can be specified multiple times)");
            Console.WriteLine("  --help, -h, -?               Display this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine("  Execute a scheduled job:");
            Console.WriteLine("    AphaBatchJobsFoundation.Host --job-name DailyReportJob --job-type Scheduled");
            Console.WriteLine();
            Console.WriteLine("  Execute an adhoc job with parameters:");
            Console.WriteLine("    AphaBatchJobsFoundation.Host --job-name DataExportJob --job-type Adhoc --param StartDate=2024-01-01 --param EndDate=2024-01-31");
            Console.WriteLine();
            Console.WriteLine("  Short form:");
            Console.WriteLine("    AphaBatchJobsFoundation.Host -j DataExportJob -t Adhoc -p StartDate=2024-01-01 -p EndDate=2024-01-31");
            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            Console.WriteLine("  0 - Success");
            Console.WriteLine("  1 - General error");
            Console.WriteLine("  2 - Configuration/validation error");
            Console.WriteLine("  3 - Database error");
            Console.WriteLine("  4 - Job not found");
            Console.WriteLine();
        }
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Code Review Changes Applied:
// 1. Sealed the class to prevent inheritance (performance optimization)
// 2. Added ConfigureAwait(false) to async calls to avoid deadlocks and improve performance
// 3. Added explicit OperationCanceledException handling for better cancellation support
// 4. Made ParseArguments and ValidateArguments static (no instance state needed)
// 5. Used StringComparer.OrdinalIgnoreCase for parameters dictionary (case-insensitive, culture-invariant)
// 6. Improved parameter parsing with explicit StringSplitOptions and validation for empty keys
// 7. Used named parameter (ignoreCase: true) in Enum.TryParse for clarity
// 8. Optimized string.Split with count parameter to avoid unnecessary allocations
//
// Design Decisions:
// 1. Constructor injection of IJobOrchestrator and AphaLogger for testability
//    and adherence to dependency inversion principle
// 2. Correlation id generation with "CLI" prefix for easy identification in logs
// 3. Comprehensive argument parsing supporting both long (--job-name) and 
//    short (-j) form arguments for better user experience
// 4. Parameter parsing supports key=value format for adhoc job parameters
// 5. Validation separated into dedicated method for clarity and maintainability
// 6. Usage display provides clear examples for both scheduled and adhoc jobs
// 7. All exceptions converted to appropriate exit codes for scheduler integration
//
// Error Handling Strategy:
// - ArgumentException for validation errors -> ConfigurationError exit code (2)
// - OperationCanceledException for cancellation -> GeneralError exit code (1)
// - General exceptions -> GeneralError exit code (1)
// - Job execution errors handled by orchestrator and returned via JobExecutionResult
// - All errors logged with correlation id for troubleshooting
//
// CLI Argument Format:
// Required:
//   --job-name <name> or -j <name>: Job identifier
//   --job-type <type> or -t <type>: Scheduled or Adhoc
// Optional:
//   --param <key=value> or -p <key=value>: Job parameters (repeatable)
//   --help or -h or -?: Display usage
//
// Exit Code Mapping:
// - 0: Successful execution
// - 1: General error (unexpected exceptions)
// - 2: Configuration/validation error (invalid arguments)
// - 3: Database error (from orchestrator)
// - 4: Job not found (from orchestrator)
//
// Logging Strategy:
// - Log CLI trigger start with argument count
// - Log job execution request with job name and type
// - Log job completion with status and exit code
// - Log all errors with correlation id and exception details
// - Use structured logging for better log analysis
//
// Thread Safety:
// - Class is stateless except for injected dependencies
// - Safe for concurrent execution with different argument sets
// - Each execution generates unique correlation id
//
// Performance Considerations:
// - Minimal overhead for argument parsing
// - Async execution throughout for non-blocking I/O
// - ConfigureAwait(false) used to avoid context switching overhead
// - No blocking calls in CLI trigger layer
// - Sealed class for potential JIT optimizations
//
// Future Extensibility:
// - Can add support for configuration file input (--config-file)
// - Can add support for JSON parameter format (--param-json)
// - Can add support for dry-run mode (--dry-run)
// - Can add support for verbose logging (--verbose)
//
// Integration Points:
// - IJobOrchestrator: Delegates actual job execution
// - AphaLogger: Structured logging with correlation id
// - ExitCodes: Standard exit codes for scheduler integration
// - JobType enum: Type-safe job type specification
//
// Apha Naming Conventions:
// - Class name: CliJobTrigger (Apha prefix in namespace)
// - Method names: PascalCase with Async suffix
// - Private fields: _camelCase with underscore prefix
// - Constants: PascalCase
//
// ============================================================================