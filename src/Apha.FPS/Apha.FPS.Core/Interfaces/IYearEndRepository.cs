using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearEndRepository
    {
        Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName);
        Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName);
        Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName);
        Task<string> GetYearEndDataSetupRequestInitiatorAsync(string jobName);
        /// <summary>
        /// Resolves the <c>JobExecutionId</c> of the single Year End Data Setup request currently in
        /// <c>Initiated</c> status, if any — the request the Confirm workflow is editing. Deliberately
        /// scoped to Data Setup only (no <c>jobName</c> parameter, unlike the generic methods above) and
        /// to <c>Initiated</c> only (not any non-terminal status) — an <c>Approved</c>/<c>Running</c>
        /// request is not editable. Returns <see langword="null"/> when none is Initiated; throws if more
        /// than one is found, since the staging model assumes exactly one request is being edited at a
        /// time.
        /// </summary>
        Task<Guid?> GetInitiatedDataSetupJobExecutionIdAsync();
        /// <summary>
        /// Creates the job_queue row for a Year End Data Setup request. <paramref name="targetFpsYear"/>
        /// is persisted as the row's target_fpsyear (CR067) - fpsyear keeps its existing meaning (the
        /// current/Open year) unchanged.
        /// </summary>
        Task<BatchJobQueue> EnqueueDataSetupInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note, int targetFpsYear);
        /// <summary>
        /// Transitions the exact Data Setup request identified by <paramref name="jobQueueId"/> to
        /// Approved. Requires the row to still be Initiated at write time (re-checked here, not just
        /// trusted from an earlier caller read) - planned-year staging design, Workstream 6.
        /// </summary>
        Task<BatchJobQueue> EnqueueDataSetupApprovalBatchJobAsync(Guid jobQueueId, string requestedBy, string note);

        /// <summary>
        /// Transitions the exact Data Setup request identified by <paramref name="jobQueueId"/> to
        /// Rejected and deletes its staged Config Value/Month Hours rows in the same transaction as the
        /// status transition, so a Rejected request can never retain editable workflow data.
        /// </summary>
        Task<BatchJobQueue> EnqueueDataSetupRejectBatchJobAsync(Guid jobQueueId, string requestedBy, string note);

        Task<bool> CanInitiateYearEndCutOverRequestAsync(string jobName);
        Task<bool> CanApproveOrRejectYearEndCutOverRequestAsync(string jobName);
        Task<string> GetYearEndCutOverRequestInitiatorAsync(string jobName);
        /// <summary>Same contract as <see cref="GetInitiatedDataSetupJobExecutionIdAsync"/>, scoped to CutOver.</summary>
        Task<Guid?> GetInitiatedCutOverJobExecutionIdAsync();
        Task<BatchJobQueue> EnqueueCutOverInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
        /// <summary>Transitions the exact CutOver request to Approved. Row must still be Initiated at write time.</summary>
        Task<BatchJobQueue> EnqueueCutOverApprovalBatchJobAsync(Guid jobExecutionId, string requestedBy, string note);
        /// <summary>Transitions the exact CutOver request to Rejected. No staged data to clean up.</summary>
        Task<BatchJobQueue> EnqueueCutOverRejectBatchJobAsync(Guid jobExecutionId, string requestedBy, string note);

        /// <summary>
        /// Records that the EventBridge publish for <paramref name="jobExecutionId"/> succeeded.
        /// Called only after IEventPublisherService.PublishAsync returns successfully - if
        /// publishing fails, the row is left Approved with triggered_at_utc still NULL, which is
        /// the durable condition a future recovery sweep depends on.
        /// </summary>
        Task SetTriggeredMetadataAsync(string jobExecutionId, string triggeredBy);
    }
}
