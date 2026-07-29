using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class YearEndService : IYearEndService
    {
        private readonly IFpsApiClient _fpsClient;

        public YearEndService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndInitiationBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
            => await _fpsClient.FpsYearEnd.GetYearEndInitiationBatchJobHistoryAsync(query, jobName);

        public async Task<ApiResponseDto<bool>> GetCanInitiateDataSetupRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.GetCanInitiateDataSetupRequestAsync(jobName);

        public async Task<ApiResponseDto<bool>> GetCanApproveDataSetupRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.GetCanApproveDataSetupRequestAsync(jobName);

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndInitiationJobAsync(int month, string correlationId)
            => await _fpsClient.FpsYearEnd.TriggerYearEndInitiationJobAsync(month, correlationId);
    }
}
