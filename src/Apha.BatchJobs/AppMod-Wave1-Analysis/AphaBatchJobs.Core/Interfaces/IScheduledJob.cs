namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Defines the contract for scheduled job implementations in the AphaBatchJobs system.
/// All scheduled jobs must implement this interface to be discoverable and executable
/// by the job orchestration framework via IEnumerable&lt;IScheduledJob&gt;.
/// </summary>
/// <remarks>
/// <para>
/// This interface is the foundation of the scheduled job execution framework.
/// Jobs implementing this interface are automatically discovered through dependency injection
/// and can be executed via the --scheduled CLI flag.
/// </para>
/// <para>
/// Implementation Guidelines:
/// - Register implementations as singletons in the Infrastructure project's DI container
/// - Use ILogger&lt;TJob&gt; for structured logging with correlation IDs
/// - Return JobExecutionResult with appropriate status, message, and exit code
/// - Handle cancellation via CancellationToken for graceful shutdown
/// - Implement timeout logic (default 300 seconds per operation)
/// - Follow exit code conventions: 0 = success, 1 = failure, 2 = timeout
/// </para>
/// <para>
/// Execution Context:
/// - Foundation: v0.1.0-foundation targeting net8.0
/// - Database: PostgreSQL via Npgsql/EntityFrameworkCore
/// - Scheduler: Quartz.NET integration
/// - Infrastructure: AWS ECS Fargate
/// - Logging: Structured logging at Info/Warning/Error levels
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class MyScheduledJob : IScheduledJob
/// {
///     private readonly ILogger&lt;MyScheduledJob&gt; _logger;
///     private readonly ICorrelationIdService _correlationIdService;
///     
///     public MyScheduledJob(
///         ILogger&lt;MyScheduledJob&gt; logger,
///         ICorrelationIdService correlationIdService)
///     {
///         _logger = logger;
///         _correlationIdService = correlationIdService;
///     }
///     
///     public async Task&lt;JobExecutionResult&gt; ExecuteAsync(
///         JobExecutionContext context,
///         CancellationToken cancellationToken = default)
///     {
///         var correlationId = _correlationIdService.GetCorrelationId();
///         _logger.LogInformation(
///             "Starting job execution. CorrelationId: {CorrelationId}",
///             correlationId);
///         
///         try
///         {
///             // Job implementation here
///             return JobExecutionResult.Success("Job completed successfully");
///         }
///         catch (OperationCanceledException)
///         {
///             _logger.LogWarning(
///                 "Job execution cancelled. CorrelationId: {CorrelationId}",
///                 correlationId);
///             return JobExecutionResult.Timeout("Job execution was cancelled");
///         }
///         catch (Exception ex)
///         {
///             _logger.LogError(
///                 ex,
///                 "Job execution failed. CorrelationId: {CorrelationId}",
///                 correlationId);
///             return JobExecutionResult.Failure($"Job failed: {ex.Message}");
///         }
///     }
/// }
/// </code>
/// </example>
public interface IScheduledJob
{
    /// <summary>
    /// Executes the scheduled job asynchronously with support for cancellation.
    /// </summary>
    /// <param name="context">
    /// The job execution context provided by the Quartz scheduler.
    /// Contains job metadata, trigger information, and execution state.
    /// Use context.JobDetail.Key to identify the specific job instance.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to support graceful shutdown and timeout handling.
    /// Implementations must monitor this token and cancel long-running operations
    /// when cancellation is requested. Timeout default is 300 seconds per operation.
    /// Defaults to <see cref="CancellationToken.None"/> if not provided.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="JobExecutionResult"/> with:
    /// <list type="bullet">
    /// <item>
    /// <term>Status</term>
    /// <description>"Success", "Failed", or "Timeout"</description>
    /// </item>
    /// <item>
    /// <term>Message</term>
    /// <description>Human-readable summary of execution outcome</description>
    /// </item>
    /// <item>
    /// <term>ExitCode</term>
    /// <description>0 for success, 1 for failure, 2 for timeout</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the cancellation token.
    /// Implementations should catch this and return JobExecutionResult.Timeout().
    /// </exception>
    /// <remarks>
    /// <para>
    /// Execution Requirements:
    /// - Log execution start with correlation ID at Information level
    /// - Log each significant step with timing information
    /// - Log warnings for retries or non-fatal issues at Warning level
    /// - Log failures with full exception details at Error level
    /// - Return appropriate JobExecutionResult based on outcome
    /// - Handle cancellation gracefully and clean up resources
    /// </para>
    /// <para>
    /// Error Handling Strategy:
    /// - Catch specific exceptions and provide meaningful error messages
    /// - Use correlation IDs for tracing across distributed systems
    /// - Never throw unhandled exceptions - always return JobExecutionResult
    /// - For multi-step jobs, halt on first failure and report which step failed
    /// </para>
    /// <para>
    /// Performance Considerations:
    /// - Monitor cancellation token regularly in long-running operations
    /// - Implement timeout logic per operation (default 300 seconds)
    /// - Use async/await properly to avoid blocking threads
    /// - Dispose resources properly using using statements or IAsyncDisposable
    /// </para>
    /// </remarks>
    Task<JobExecutionResult> ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default);
}


**Changes Made:**

1. **Added default parameter value for CancellationToken**: Changed `CancellationToken cancellationToken` to `CancellationToken cancellationToken = default` following .NET 8 best practices for optional cancellation tokens in interface definitions. This provides better API ergonomics while maintaining backward compatibility.

2. **Updated XML documentation**: Added clarification in the `<param>` documentation for `cancellationToken` to explicitly mention the default value behavior: "Defaults to <see cref="CancellationToken.None"/> if not provided."

3. **Updated example code**: Modified the example implementation to include `cancellationToken = default` to demonstrate the correct usage pattern for implementers.

These changes align with .NET 8 best practices where `CancellationToken` parameters should have default values in interface definitions to make them optional while still encouraging proper cancellation support in implementations.