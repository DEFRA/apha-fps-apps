using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces.Adhoc
{
    /// <summary>
    /// Defines the contract for adhoc job implementations in the AphaBatchJobs system.
    /// All adhoc jobs must implement this interface to be discoverable and executable
    /// by the job orchestration framework via IEnumerable&lt;IAdhocJob&gt;.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is the foundation of the adhoc job execution framework.
    /// Jobs implementing this interface are automatically discovered through dependency injection
    /// and can be executed on-demand via the --adhoc CLI flag.
    /// </para>
    /// <para>
    /// Implementation Guidelines:
    /// - Register implementations as singletons in the Infrastructure project's DI container
    /// - Use ILogger&lt;TAdhocJob&gt; for structured logging with correlation IDs
    /// - Return JobExecutionResult with appropriate status, message, and exit code
    /// - Handle cancellation via CancellationToken for graceful shutdown
    /// - Implement timeout logic (default 300 seconds per operation)
    /// - Follow exit code conventions: 0 = success, 1 = failure, 2 = timeout
    /// </para>
    /// <para>
    /// Execution Context:
    /// - Foundation: v0.1.0-foundation targeting net8.0
    /// - Database: PostgreSQL via Npgsql/EntityFrameworkCore
    /// - Infrastructure: AWS ECS Fargate
    /// - Logging: Structured logging at Info/Warning/Error levels
    /// </para>
    /// </remarks>
    public interface IAdhocJob
    {
        /// <summary>
        /// Executes the adhoc job asynchronously with support for cancellation.
        /// </summary>
        /// <param name="cancellationToken">
        /// Cancellation token to support graceful shutdown and timeout handling.
        /// Implementations must monitor this token and cancel long-running operations
        /// when cancellation is requested. Timeout default is 300 seconds per operation.
        /// Defaults to <see cref="CancellationToken.None"/> if not provided.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with:
        /// - Status: "Success", "Failed", or "Timeout"
        /// - Message: Human-readable description of the result
        /// - ExitCode: 0 for success, 1 for failure, 2 for timeout
        /// </returns>
        Task<JobExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
