using System.Threading.Tasks;
using AphaBatchJobsWave2AdhocRecreateSummaries.Models;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Interfaces
{
    /// <summary>
    /// Defines the contract for adhoc jobs that can be executed with month parameter and correlation tracking.
    /// Implementations are discovered via IEnumerable&lt;IAdhocJob&gt; for dynamic job registration.
    /// </summary>
    public interface IAdhocJob
    {
        /// <summary>
        /// Gets the unique name identifier for this adhoc job.
        /// This should be a stable identifier used for job registration and logging.
        /// </summary>
        string JobName { get; }

        /// <summary>
        /// Executes the adhoc job asynchronously with the specified month parameter and correlation identifier.
        /// </summary>
        /// <param name="month">The month parameter (1-12) for job execution.</param>
        /// <param name="correlationId">The correlation identifier for tracking and logging purposes.</param>
        /// <param name="cancellationToken">Cancellation token to support graceful shutdown and task cancellation.</param>
        /// <returns>A task representing the asynchronous operation with JobExecutionResult containing status, message, and exit code.</returns>
        Task<JobExecutionResult> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default);
    }
}


**Key improvements made:**

1. **CancellationToken Support**: Added `CancellationToken` parameter with default value to support graceful shutdown scenarios in ECS Fargate and proper async operation cancellation - a .NET 8 best practice for async methods.

2. **Enhanced Documentation**: Improved XML documentation for `JobName` property to clarify its purpose as a stable identifier.

3. **Async Best Practices**: The cancellation token enables proper cooperative cancellation, which is essential for:
   - ECS Fargate graceful shutdown handling
   - Quartz job interruption support
   - Long-running operations that need to be cancelled
   - Resource cleanup and timeout scenarios

4. **Backward Compatibility**: Using `default` parameter ensures existing implementations don't break while allowing new implementations to leverage cancellation support.