using System.Threading;
using System.Threading.Tasks;

namespace AphaBatchJobs.Core.Interfaces.Adhoc
{
    /// <summary>
    /// Orchestrator service for the RecreateSummaries adhoc job.
    /// Coordinates execution of 24 procedures in the proper sequence:
    /// 16 core procedures (sp_RecreateSummaries chain) + 8 email notifications.
    /// </summary>
    public interface IRecreateSummariesService
    {
        /// <summary>
        /// Executes the complete RecreateSummaries workflow asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
        /// <returns>True if all 24 procedures executed successfully, false otherwise.</returns>
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
