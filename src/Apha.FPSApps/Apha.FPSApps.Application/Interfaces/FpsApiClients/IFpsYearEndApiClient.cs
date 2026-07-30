using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsYearEndApiClient
    {
        Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndInitiationBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> GetCanInitiateDataSetupRequestAsync(string jobName);
        Task<ApiResponseDto<bool>> GetCanApproveDataSetupRequestAsync(string jobName);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndInitiationJobAsync(int plannedYear);
    }
}
