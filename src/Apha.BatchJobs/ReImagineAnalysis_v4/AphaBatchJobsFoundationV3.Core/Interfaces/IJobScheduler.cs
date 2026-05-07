using System.Threading;
using System.Threading.Tasks;

namespace AphaBatchJobsFoundationV3.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for job scheduling service to schedule and manage batch job executions.
    /// Provides methods to start and stop the job scheduler with support for graceful cancellation.
    /// </summary>
    public interface IJobScheduler
    {
        /// <summary>
        /// Starts the job scheduler asynchronously.
        /// Initializes and begins scheduling configured batch jobs for execution.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests during startup.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous start operation.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the job scheduler asynchronously with graceful shutdown.
        /// Ensures all running jobs complete or are properly cancelled before shutdown.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests during shutdown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous stop operation.</returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}


// Changes made:
// 1. Updated XML documentation to use "Defines" instead of "Interface defining" - more concise and idiomatic
// 2. Added "The" article before parameter descriptions for consistency with .NET documentation standards
// 3. Added <see cref="Task"/> references in returns documentation for better IntelliSense and documentation linking
// 4. Changed "A task" to "A <see cref="Task"/>" for proper type referencing in XML documentation
// 5. Maintained all existing functionality without adding new features