namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Interface for job orchestrators that coordinate the execution flow of batch jobs.
/// Orchestrators are responsible for managing the lifecycle of job execution including
/// pre-execution setup, execution coordination, post-execution cleanup, and error handling.
/// This abstraction allows different orchestration strategies for scheduled vs adhoc jobs
/// while maintaining a consistent execution contract across the AphaBatchJobs platform.
/// </summary>
/// <remarks>
/// Implementations of this interface should:
/// - Handle job execution lifecycle management
/// - Coordinate with database connections and transactions
/// - Manage correlation ID propagation for distributed tracing
/// - Implement retry logic based on DatabaseOptions configuration
/// - Ensure proper resource cleanup in AWS ECS Fargate containerized environments
/// - Return appropriate exit codes for container orchestration
/// </remarks>
public interface IJobOrchestrator
{
    /// <summary>
    /// Executes the orchestrated job flow asynchronously.
    /// This method coordinates all aspects of job execution including initialization,
    /// execution, error handling, and result aggregation.
    /// </summary>
    /// <param name="context">
    /// The execution context containing job metadata including job name, correlation ID,
    /// trigger type (Scheduled or Adhoc), and start timestamp. This context is used for
    /// logging, tracing, and auditing throughout the execution lifecycle.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to support graceful shutdown in containerized environments.
    /// ECS Fargate sends SIGTERM signals during task termination which should be
    /// propagated through this token to allow jobs to complete or rollback cleanly.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a JobExecutionResult with status, descriptive message, and exit code.
    /// Exit code 0 indicates success, non-zero values indicate various failure scenarios
    /// as defined by ExitCodeMapper for proper container orchestration feedback.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when context parameter is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the cancellationToken,
    /// typically during container shutdown or timeout scenarios.
    /// </exception>
    Task<JobExecutionResult> ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Removed "System." prefix from exception types in XML documentation (ArgumentNullException, OperationCanceledException)
//    - This follows .NET documentation conventions where exception types are referenced without namespace prefix
// 2. Added default value for cancellationToken parameter (= default)
//    - This is a .NET best practice for CancellationToken parameters, making the API more convenient to use
//    - Allows callers to omit the parameter when cancellation support is not needed
//    - Aligns with standard .NET async method patterns (e.g., HttpClient, DbContext methods)
// 3. Removed trailing space after "context," parameter for consistent formatting
// 4. Added period at end of ArgumentNullException documentation for consistency