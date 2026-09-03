using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsYearEndApiClient
    {
        Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndDataSetupBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> CanInitiateDataSetupRequestAsync(string jobName);
        Task<ApiResponseDto<bool>> CanApproveOrRejectDataSetupRequestAsync(string jobName);
        Task<ApiResponseDto<Guid?>> GetInitiatedDataSetupJobExecutionIdAsync();
        Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndDataSetupApprovalJobAsync(int plannedYear, Guid jobExecutionId);
        Task<ApiResponseDto<bool>> EnqueueYearEndDataSetupRejectJobAsync(int plannedYear, Guid jobExecutionId);

        Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndCutOverBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> CanInitiateCutOverRequestAsync(string jobName);
        Task<ApiResponseDto<bool>> CanApproveOrRejectCutOverRequestAsync(string jobName);
        Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndCutOverInitiationJobAsync(int plannedYear);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndCutOverApprovalJobAsync(int plannedYear);
        Task<ApiResponseDto<bool>> EnqueueYearEndCutOverRejectJobAsync(int plannedYear);
    }
}
