using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsYearEndApiClient
    {
        Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndDataSetupBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> GetCanInitiateDataSetupRequestAsync(string jobName);
        Task<ApiResponseDto<bool>> GetCanApproveOrRejectDataSetupRequestAsync(string jobName);
        Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndDataSetupApprovalJobAsync(int plannedYear);
        Task<ApiResponseDto<bool>> EnqueueYearEndDataSetupRejectJobAsync(int plannedYear);
        Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndCutOverBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> GetCanInitiateCutOverRequestAsync(string jobName);
        Task<ApiResponseDto<bool>> GetCanApproveCutOverRequestAsync(string jobName);
        Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndCutOverInitiationJobAsync(int plannedYear);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndCutOverApprovalJobAsync(int plannedYear);
    }
}
