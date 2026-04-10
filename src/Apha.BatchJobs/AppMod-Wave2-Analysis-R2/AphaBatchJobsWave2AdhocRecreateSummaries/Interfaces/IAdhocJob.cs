using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsWave2AdhocRecreateSummaries.Models;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Interfaces
{
    /// <summary>
    /// Defines the contract for adhoc jobs that can be executed on-demand.
    /// Implementations must validate input, enforce timeouts, and return structured results.
    /// </summary>
    public interface IAdhocJob
    {
        /// <summary>
        /// Gets the unique name identifier for this adhoc job.
        /// Used for job discovery and registration.
        /// </summary>
        string JobName { get; }

        /// <summary>
        /// Executes the adhoc job with the specified month parameter.
        /// </summary>
        /// <param name="month">The month number (1-12) for which to execute the job.</param>
        /// <param name="cancellationToken">Cancellation token to support graceful shutdown and timeout enforcement.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="JobExecutionResult"/> with:
        /// <list type="bullet">
        /// <item><description>Status: Success, Failed, or Timeout</description></item>
        /// <item><description>Message: Descriptive outcome message</description></item>
        /// <item><description>ExitCode: 0 (success), 1 (failure), or 2 (timeout)</description></item>
        /// <item><description>CorrelationId: Unique identifier for tracking</description></item>
        /// <item><description>Timing information: StartedAt, CompletedAt, Duration</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>Implementations must adhere to the following requirements:</para>
        /// <list type="bullet">
        /// <item><description>Validate that month is in the range 1-12 (inclusive)</description></item>
        /// <item><description>Generate and log a unique correlation ID for request tracing</description></item>
        /// <item><description>Enforce per-step timeout of 300 seconds using the provided cancellation token</description></item>
        /// <item><description>Stop execution immediately upon first failure (fail-fast pattern)</description></item>
        /// <item><description>Log comprehensive step information: start time, end time, duration, and result</description></item>
        /// <item><description>Return appropriate exit code: 0 (success), 1 (failure), 2 (timeout)</description></item>
        /// <item><description>Handle <see cref="OperationCanceledException"/> for graceful cancellation</description></item>
        /// <item><description>Ensure proper resource cleanup in all execution paths</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when month parameter is not between 1 and 12.
        /// </exception>
        Task<JobExecutionResult> ExecuteAsync(int month, CancellationToken cancellationToken = default);
    }
}


// Key improvements made:
// 1. Renamed method from 'Execute' to 'ExecuteAsync' following .NET async naming conventions
// 2. Enhanced XML documentation with proper formatting using <list> and <para> tags
// 3. Added <see cref> tags for better IntelliSense and documentation linking
// 4. Added <exception> documentation to specify expected validation exceptions
// 5. Expanded remarks to include explicit handling of OperationCanceledException
// 6. Added requirement for proper resource cleanup in all execution paths
// 7. Improved readability of documentation with structured lists
// 8. Made cancellation token usage more explicit in documentation
// 9. Clarified fail-fast pattern requirement for better implementation guidance
// 10. Maintained all existing functionality without adding new features