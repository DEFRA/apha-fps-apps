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

        public async Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndDataSetupBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
            => await _fpsClient.FpsYearEnd.GetYearEndDataSetupBatchJobHistoryAsync(query, jobName);

        public async Task<ApiResponseDto<bool>> CanInitiateDataSetupRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.CanInitiateDataSetupRequestAsync(jobName);

        public async Task<ApiResponseDto<bool>> CanApproveOrRejectDataSetupRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.CanApproveOrRejectDataSetupRequestAsync(jobName);

        public async Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear)
            => await _fpsClient.FpsYearEnd.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndDataSetupApprovalJobAsync(int plannedYear)
            => await _fpsClient.FpsYearEnd.TriggerYearEndDataSetupApprovalJobAsync(plannedYear);

        public async Task<ApiResponseDto<bool>> EnqueueYearEndDataSetupRejectJobAsync(int plannedYear)
           => await _fpsClient.FpsYearEnd.EnqueueYearEndDataSetupRejectJobAsync(plannedYear);

        public async Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndCutOverBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
            => await _fpsClient.FpsYearEnd.GetYearEndCutOverBatchJobHistoryAsync(query, jobName);

        public async Task<ApiResponseDto<bool>> CanInitiateCutOverRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.CanInitiateCutOverRequestAsync(jobName);

        public async Task<ApiResponseDto<bool>> CanApproveOrRejectCutOverRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.CanApproveOrRejectCutOverRequestAsync(jobName);

        public async Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndCutOverInitiationJobAsync(int plannedYear)
       => await _fpsClient.FpsYearEnd.EnqueueYearEndCutOverInitiationJobAsync(plannedYear);

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndCutOverApprovalJobAsync(int plannedYear)
            => await _fpsClient.FpsYearEnd.TriggerYearEndCutOverApprovalJobAsync(plannedYear);

        public async Task<ApiResponseDto<bool>> EnqueueYearEndCutOverRejectJobAsync(int plannedYear)
            => await _fpsClient.FpsYearEnd.EnqueueYearEndCutOverRejectJobAsync(plannedYear);

        // Temporary Year End test-reset tool - see Apha.FPS.Api/TestTools. Delete alongside it.
        public async Task<ApiResponseDto<bool>> IsYearEndTestResetEnabledAsync()
            => await _fpsClient.FpsYearEnd.IsYearEndTestResetEnabledAsync();

        public async Task<ApiResponseDto<bool>> TriggerYearEndTestReset2026Async()
            => await _fpsClient.FpsYearEnd.TriggerYearEndTestReset2026Async();
    }
}