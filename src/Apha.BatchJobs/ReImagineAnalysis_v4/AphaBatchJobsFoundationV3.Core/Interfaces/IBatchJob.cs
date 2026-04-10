using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundationV3.Core.Enums;

namespace AphaBatchJobsFoundationV3.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for batch job execution.
    /// All batch jobs must implement this interface to be executed by the scheduler or CLI trigger.
    /// </summary>
    public interface IBatchJob
    {
        /// <summary>
        /// Gets the unique name identifier for the batch job.
        /// This name is used for logging, scheduling, and CLI invocation.
        /// </summary>
        string JobName { get; }

        /// <summary>
        /// Executes the batch job asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// Implementations should monitor this token and gracefully terminate when cancellation is requested.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
        /// containing an <see cref="ExitCode"/> indicating the execution outcome.
        /// </returns>
        Task<ExitCode> ExecuteAsync(CancellationToken cancellationToken);
    }
}


// Changes made:
// 1. Simplified XML documentation comments - removed redundant <value> tag for property (not commonly used in modern C#)
// 2. Removed verbose explanatory text from interface summary - interfaces should be concise
// 3. Streamlined method documentation - removed implementation details from interface (those belong in implementing classes)
// 4. Added proper XML documentation cross-references using <see cref=""/> tags for better IntelliSense support
// 5. Improved returns documentation formatting for better readability
// 6. Maintained all existing functionality and structure - no features added or removed