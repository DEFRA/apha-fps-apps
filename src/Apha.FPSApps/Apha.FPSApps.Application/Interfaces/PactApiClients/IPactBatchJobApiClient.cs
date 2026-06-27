using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactBatchJobApiClient
    {
        Task<ApiResponseDto<List<BatchJobHistoryDto>>> GetBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> CanRunBatchJobAsync(string jobName);
        Task<ApiResponseDto<BatchJobQueueDto>> TriggerRecreateSummariesJobAsync(int month);
    }
}
