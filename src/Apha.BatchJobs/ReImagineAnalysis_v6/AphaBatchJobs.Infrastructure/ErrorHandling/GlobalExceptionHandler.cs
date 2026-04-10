using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using AphaBatchJobs.Core.Enums;
using AphaBatchJobs.Infrastructure.ErrorHandling;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AphaBatchJobs.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Service class for centralized exception handling with correlation logging and exit code mapping.
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly ICorrelationIdService _correlationIdService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GlobalExceptionHandler class.
        /// </summary>
        /// <param name="correlationIdService">The correlation ID service for tracking operations.</param>
        /// <param name="logger">The logger for recording exception information.</param>
        public GlobalExceptionHandler(ICorrelationIdService correlationIdService, ILogger<GlobalExceptionHandler> logger)
        {
            _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles exceptions by logging with correlation ID and returning appropriate JobExecutionResult.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="context">The job execution context.</param>
        /// <returns>A JobExecutionResult with failed status and appropriate exit code.</returns>
        public Task<JobExecutionResult> HandleExceptionAsync(Exception exception, JobExecutionContext context)
        {
            // Best Practice: Validate input parameters
            ArgumentNullException.ThrowIfNull(exception);
            
            LogException(exception, context);
            var result = CreateFailureResult(exception);
            
            // Best Practice: Use Task.FromResult directly without await for synchronous operations
            return Task.FromResult(result);
        }

        /// <summary>
        /// Logs exception details with correlation ID and job context information.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="context">The job execution context.</param>
        private void LogException(Exception exception, JobExecutionContext context)
        {
            // Best Practice: Use null-coalescing operator chain for cleaner null handling
            var correlationId = _correlationIdService.GetCorrelationId() ?? context?.CorrelationId ?? "N/A";
            
            // Best Practice: Use structured logging with proper null handling
            _logger.LogError(
                exception,
                "Job execution failed. CorrelationId: {CorrelationId}, JobName: {JobName}, JobType: {JobType}, StartedAt: {StartedAt}, ExceptionType: {ExceptionType}, ExceptionMessage: {ExceptionMessage}",
                correlationId,
                context?.JobName ?? "Unknown",
                context?.JobType.ToString() ?? "Unknown",
                context?.StartedAt.ToString("o") ?? "Unknown",
                exception.GetType().Name,
                exception.Message
            );
        }

        /// <summary>
        /// Creates a JobExecutionResult with failed status and mapped exit code.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <returns>A JobExecutionResult representing the failure.</returns>
        private JobExecutionResult CreateFailureResult(Exception exception)
        {
            var exitCode = ExitCodeMapper.MapExceptionToExitCode(exception);
            var exitCodeDescription = ExitCodeMapper.GetExitCodeDescription(exitCode);

            // Best Practice: Use object initializer for cleaner instantiation
            return new JobExecutionResult
            {
                Status = JobExecutionStatus.Failed,
                ExitCode = exitCode,
                Message = $"{exitCodeDescription}: {exception.Message}",
                CompletedAt = DateTimeOffset.UtcNow,
                Exception = exception
            };
        }
    }
}


// Key improvements made:
// 1. Removed unnecessary 'await' in HandleExceptionAsync - the method is synchronous, so Task.FromResult is sufficient
// 2. Added ArgumentNullException.ThrowIfNull for exception parameter validation (modern .NET pattern)
// 3. Removed redundant null checks on exception properties (exception.GetType().Name and exception.Message) since exception is validated
// 4. Maintained all existing functionality without adding new features
// 5. Improved code efficiency by removing unnecessary async/await state machine overhead
// 6. Added inline comments explaining best practices applied