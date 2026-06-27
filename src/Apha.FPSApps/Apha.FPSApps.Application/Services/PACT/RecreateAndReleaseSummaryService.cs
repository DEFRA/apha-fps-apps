using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class RecreateAndReleaseSummaryService : IRecreateAndReleaseSummaryService
    {
        private readonly IPactApiClient _pactClient;

        public RecreateAndReleaseSummaryService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>> GetRecreateSummaryLogAsync(QueryParameters<string> query)
            => await _pactClient.PactRecreateSummaryLog.GetRecreateSummaryLogAsync(query);

        public async Task<ApiResponseDto<List<BatchJobHistoryDto>>> GetBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
            => await _pactClient.PactBatchJob.GetBatchJobHistoryAsync(query, jobName);

        public async Task<ApiResponseDto<bool>> CanRunBatchJobAsync(string jobName)
            => await _pactClient.PactBatchJob.CanRunBatchJobAsync(jobName);

        public async Task<ApiResponseDto<BatchJobQueueDto>> TriggerRecreateSummariesJobAsync(int month)
            => await _pactClient.PactBatchJob.TriggerRecreateSummariesJobAsync(month);
    }
}
