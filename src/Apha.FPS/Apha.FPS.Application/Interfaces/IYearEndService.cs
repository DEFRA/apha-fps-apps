using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IYearEndService
    {
        Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName);

        Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName);
        Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName);
        /// <summary>
        /// Resolves the <c>JobExecutionId</c> of the single Year End Data Setup request currently in
        /// <c>Initiated</c> status, if any - the request the Confirm workflow is editing. Returns
        /// <see langword="null"/> when none is Initiated.
        /// </summary>
        Task<Guid?> GetInitiatedDataSetupJobExecutionIdAsync();
        Task<BatchJobQueueDto> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear, int contextyear, string requestedBy, string correlationId);
        Task<BatchJobEventTriggerDto> EnqueueYearEndDataSetupApprovalJobAsync(Guid jobExecutionId, int plannedYear, int contextYear, string requestedBy, string correlationId);
        Task<bool> EnqueueYearEndDataSetupRejectJobAsync(Guid jobExecutionId, int plannedYear, int contextYear, string requestedBy, string correlationId);

        Task<bool> CanInitiateYearEndCutOverRequestAsync(string jobName);
        Task<bool> CanApproveOrRejectYearEndCutOverRequestAsync(string jobName);
        /// <summary>The CutOver equivalent of <see cref="GetInitiatedDataSetupJobExecutionIdAsync"/>.</summary>
        Task<Guid?> GetInitiatedCutOverJobExecutionIdAsync();
        Task<BatchJobQueueDto> EnqueueYearEndCutOverInitiationJobAsync(int plannedYear, int contextyear, string requestedBy, string correlationId);
        Task<BatchJobEventTriggerDto> EnqueueYearEndCutOverApprovalJobAsync(Guid jobExecutionId, int plannedYear, int contextYear, string requestedBy, string correlationId);
        Task<bool> EnqueueYearEndCutOverRejectJobAsync(Guid jobExecutionId, int plannedYear, int contextYear, string requestedBy, string correlationId);
    }
}
