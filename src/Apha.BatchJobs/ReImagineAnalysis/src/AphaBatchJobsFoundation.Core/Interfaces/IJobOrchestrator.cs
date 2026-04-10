// ============================================================================
// File: IJobOrchestrator.cs
// Description: Interface contract for job orchestration to execute scheduled 
//              and adhoc jobs with proper async patterns for Apha BatchJobs Foundation
// ============================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundation.Core.Models;

namespace AphaBatchJobsFoundation.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for job orchestration in the Apha BatchJobs Foundation.
    /// Provides methods to execute both scheduled and adhoc jobs with proper async patterns,
    /// error handling, and cancellation support.
    /// Implementations should remain thin and delegate business logic to appropriate services.
    /// </summary>
    public interface IJobOrchestrator
    {
        /// <summary>
        /// Orchestrates the execution of a scheduled job asynchronously.
        /// Scheduled jobs are triggered by external schedulers or time-based triggers
        /// and typically run on a predefined schedule without additional parameters.
        /// </summary>
        /// <param name="jobName">
        /// The unique name identifier of the scheduled job to execute.
        /// Must match a registered job name in the job registry.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token to support graceful shutdown and job cancellation.
        /// Allows external systems to request job termination.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with execution status,
        /// scheduler-friendly exit code, completion metadata, and error details if applicable.
        /// </returns>
        /// <remarks>
        /// Implementation guidelines:
        /// - Validate jobName parameter before execution
        /// - Log job start and completion with correlation id
        /// - Handle exceptions and convert to appropriate JobExecutionResult
        /// - Respect cancellation token throughout execution
        /// - Return scheduler-friendly exit codes (0 for success, non-zero for failures)
        /// - Keep orchestration logic thin, delegate to services
        /// </remarks>
        Task<JobExecutionResult> ExecuteScheduledJobAsync(string jobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Orchestrates the execution of an adhoc job asynchronously with custom parameters.
        /// Adhoc jobs are triggered manually via CLI or API and accept dynamic parameters
        /// for flexible execution scenarios.
        /// </summary>
        /// <param name="jobName">
        /// The unique name identifier of the adhoc job to execute.
        /// Must match a registered job name in the job registry.
        /// </param>
        /// <param name="parameters">
        /// Dictionary of key-value pairs containing job-specific parameters.
        /// Parameter names and types should be validated by the job implementation.
        /// Can be null or empty if the job does not require parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token to support graceful shutdown and job cancellation.
        /// Allows external systems to request job termination.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with execution status,
        /// scheduler-friendly exit code, completion metadata, and error details if applicable.
        /// </returns>
        /// <remarks>
        /// Implementation guidelines:
        /// - Validate jobName and parameters before execution
        /// - Log job start with parameters and completion with correlation id
        /// - Handle exceptions and convert to appropriate JobExecutionResult
        /// - Respect cancellation token throughout execution
        /// - Return scheduler-friendly exit codes (0 for success, non-zero for failures)
        /// - Keep orchestration logic thin, delegate to services
        /// - Validate parameter types and required parameters
        /// </remarks>
        Task<JobExecutionResult> ExecuteAdhocJobAsync(
            string jobName, 
            IDictionary<string, object> parameters, 
            CancellationToken cancellationToken = default);
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Design Decisions:
// 1. Interface defines two distinct methods for scheduled and adhoc job execution
//    to support different triggering mechanisms and parameter handling
// 2. Both methods return Task<JobExecutionResult> for consistent async patterns
//    and unified result handling across job types
// 3. CancellationToken parameter enables graceful shutdown and external cancellation
//    support for long-running jobs
// 4. IDictionary<string, object> for adhoc parameters provides flexibility for
//    different parameter types while maintaining type safety through validation
// 5. String-based jobName allows for simple job identification and registry lookup
//
// Code Improvements Applied:
// 1. Added default parameter value (= default) for CancellationToken in both methods
//    - Follows .NET best practices for optional cancellation tokens
//    - Simplifies method calls when cancellation is not needed
//    - Maintains backward compatibility
// 2. Changed Dictionary<string, object> to IDictionary<string, object>
//    - Follows interface segregation principle
//    - Allows more flexible implementations (Dictionary, ConcurrentDictionary, etc.)
//    - Reduces coupling to concrete implementation
//    - Standard .NET practice for method parameters
//
// Orchestration Responsibilities:
// - Job identification and validation
// - Correlation id generation and propagation
// - Structured logging of execution lifecycle
// - Exception handling and conversion to JobExecutionResult
// - Cancellation token propagation
// - Exit code determination for scheduler integration
//
// Orchestration Should NOT:
// - Contain business logic (delegate to services)
// - Perform data access directly (use repositories through services)
// - Handle infrastructure concerns (logging, config handled by DI)
//
// Integration Points:
// - Scheduler Integration: External schedulers call ExecuteScheduledJobAsync
// - CLI Manual Trigger: Command-line interface calls ExecuteAdhocJobAsync
// - Job Registry: Implementations resolve job instances by jobName
// - Logging Infrastructure: Correlation id and structured logging
// - Configuration: Job-specific settings loaded from configuration
//
// Error Handling Strategy:
// - Validation errors: Return JobExecutionResult.Failure with exit code 1
// - Job not found: Return JobExecutionResult.Failure with exit code 1
// - Execution errors: Return JobExecutionResult.Failure with exception details
// - Cancellation: Return JobExecutionResult.Cancelled with exit code 2
// - Success: Return JobExecutionResult.Success with exit code 0
//
// Thread Safety:
// - Interface methods should be safe to call concurrently
// - Implementations must handle concurrent job executions appropriately
// - Job instances should be resolved per execution to avoid state sharing
//
// Performance Considerations:
// - All I/O operations must be async
// - Avoid blocking calls in orchestration layer
// - Use ConfigureAwait(false) in implementations where appropriate
// - Consider timeout policies for long-running jobs
//
// Naming Conventions:
// - Follows Apha naming patterns with "Apha" prefix
// - Interface name starts with "I" prefix
// - Async suffix on method names following .NET conventions
// - Clear, descriptive parameter names
//
// Future Extensibility:
// - Interface can be extended with additional methods for:
//   * Batch job execution
//   * Job status querying
//   * Job cancellation by execution id
//   * Job scheduling management
// - Consider versioning strategy if breaking changes needed
//
// ============================================================================


**Summary of Changes:**

1. **Added `= default` to CancellationToken parameters**: This is a .NET best practice that makes the cancellation token optional while maintaining the same functionality. Callers can omit the parameter when cancellation support is not needed.

2. **Changed `Dictionary<string, object>` to `IDictionary<string, object>`**: Using the interface type instead of the concrete implementation follows the Dependency Inversion Principle and is a standard .NET best practice. This allows implementations to accept any dictionary type (Dictionary, ConcurrentDictionary, ReadOnlyDictionary, etc.) and reduces coupling.

These changes improve the API design without altering functionality, making the interface more flexible and aligned with .NET conventions.