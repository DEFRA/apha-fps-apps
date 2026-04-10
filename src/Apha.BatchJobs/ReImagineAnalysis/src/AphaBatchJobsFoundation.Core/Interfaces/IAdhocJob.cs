// ============================================================================
// File: IAdhocJob.cs
// Description: Interface contract for adhoc job execution with async execution 
//              method and job identification for Apha BatchJobs Foundation
// ============================================================================

using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundation.Core.Models;

namespace AphaBatchJobsFoundation.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for adhoc batch jobs in the Apha BatchJobs system.
    /// Adhoc jobs are triggered manually via CLI or on-demand through external systems,
    /// as opposed to scheduled jobs that run on a predefined schedule.
    /// </summary>
    /// <remarks>
    /// Implementing classes should:
    /// <list type="bullet">
    /// <item><description>Provide a unique job name for identification and registration</description></item>
    /// <item><description>Implement async execution logic with proper cancellation support</description></item>
    /// <item><description>Return structured execution results for scheduler integration</description></item>
    /// <item><description>Handle exceptions gracefully and return appropriate failure results</description></item>
    /// <item><description>Support correlation tracking through the execution context</description></item>
    /// </list>
    /// </remarks>
    public interface IAdhocJob
    {
        /// <summary>
        /// Gets the unique name identifier for the adhoc job.
        /// This name is used for job registration, CLI invocation, and logging.
        /// Must be unique across all registered adhoc jobs in the system.
        /// </summary>
        /// <value>
        /// A string representing the unique job name.
        /// Should follow Apha naming conventions (e.g., "DataMigrationJob", "ReportGenerationJob").
        /// </value>
        /// <example>
        /// <code>
        /// public string JobName => "DataMigrationJob";
        /// </code>
        /// </example>
        string JobName { get; }

        /// <summary>
        /// Executes the adhoc job asynchronously with the provided execution context.
        /// This method contains the core business logic for the adhoc job.
        /// </summary>
        /// <param name="context">
        /// The job execution context containing correlation ID, job parameters,
        /// execution metadata, and other contextual information required for job execution.
        /// Cannot be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the job execution.
        /// Implementations should honor cancellation requests and return a cancelled result.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with execution status,
        /// exit code, completion timestamp, and any error details.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when context is null (implementation responsibility).
        /// </exception>
        /// <remarks>
        /// Implementation guidelines:
        /// <list type="bullet">
        /// <item><description>Use async/await for all I/O and database operations</description></item>
        /// <item><description>Check cancellationToken.IsCancellationRequested periodically for long-running operations</description></item>
        /// <item><description>Return JobExecutionResult.Success() for successful completion</description></item>
        /// <item><description>Return JobExecutionResult.Failure() with appropriate error details on failure</description></item>
        /// <item><description>Return JobExecutionResult.Cancelled() when cancellation is requested</description></item>
        /// <item><description>Log using structured logging with the correlation ID from context</description></item>
        /// <item><description>Delegate business logic to service layer, keep orchestration thin</description></item>
        /// <item><description>Handle exceptions gracefully and return failure results instead of throwing</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// public async Task&lt;JobExecutionResult&gt; ExecuteAsync(
        ///     JobExecutionContext context, 
        ///     CancellationToken cancellationToken)
        /// {
        ///     if (context == null)
        ///         return JobExecutionResult.Failure("Context cannot be null");
        ///     
        ///     try
        ///     {
        ///         // Business logic here
        ///         return JobExecutionResult.Success();
        ///     }
        ///     catch (Exception ex)
        ///     {
        ///         return JobExecutionResult.Failure(ex.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Design Decisions:
// 1. Interface follows .NET naming convention with 'I' prefix
// 2. JobName as property (not method) for cleaner syntax and consistency
// 3. ExecuteAsync signature matches requirement specification exactly
// 4. CancellationToken support for graceful shutdown and timeout handling
// 5. Returns Task<JobExecutionResult> for scheduler-friendly integration
//
// Usage Pattern:
// - Adhoc jobs are registered in DI container during startup
// - CLI triggers resolve job by JobName and invoke ExecuteAsync
// - Orchestrator passes JobExecutionContext with correlation ID and parameters
// - Job implementation delegates to service layer for business logic
// - Result is used to determine process exit code for scheduler integration
//
// Difference from Scheduled Jobs:
// - Adhoc jobs: Triggered manually or on-demand
// - Scheduled jobs: Run automatically on predefined schedule
// - Both share similar execution contract but different trigger mechanisms
//
// Integration Points:
// - CLI: Resolves IAdhocJob by JobName from DI container
// - Orchestrator: Creates JobExecutionContext and manages execution lifecycle
// - Logging: Uses CorrelationId from context for distributed tracing
// - Scheduler: Uses ExitCode from result for success/failure determination
//
// Thread Safety:
// - Interface contract does not guarantee thread safety
// - Implementations should be registered as transient or scoped in DI
// - Avoid shared mutable state in job implementations
//
// Error Handling:
// - Implementations should catch exceptions and return failure results
// - Unhandled exceptions should be caught by orchestrator layer
// - Use structured logging for error details with correlation ID
//
// Best Practices Applied:
// - Enhanced XML documentation with proper list formatting for better readability
// - Added example code snippets in XML documentation for implementation guidance
// - Added exception documentation for ArgumentNullException
// - Improved remarks formatting using bullet lists
// - Added value example for JobName property
// - Maintained interface segregation principle (single responsibility)
// - Follows async/await pattern consistently
// - CancellationToken as last parameter (standard .NET convention)
//
// ============================================================================


**Review Summary:**

The code has been refined with the following .NET best practices improvements:

1. **Enhanced XML Documentation**: Added proper `<list>` tags with bullet points for better documentation rendering in IDEs
2. **Code Examples**: Added `<example>` sections with sample implementations for both JobName property and ExecuteAsync method
3. **Exception Documentation**: Added `<exception>` tag to document potential ArgumentNullException
4. **Improved Readability**: Restructured remarks sections using proper list formatting
5. **Consistency**: Maintained consistent documentation style throughout
6. **Standards Compliance**: Ensured all XML documentation follows Microsoft's recommended practices

The interface design itself is solid and follows .NET conventions properly. No functional changes were needed, only documentation enhancements for better developer experience.