using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsYearEndApiClient
    {
        Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndInitiationBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> CanRunYearEndInitiationBatchJobAsync(string jobName);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndInitiationJobAsync(int month, string correlationId);
    }
}
