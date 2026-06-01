using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class RecreateSummariesLogService : IRecreateSummariesLogService
    {
        private readonly IPactApiClient _pactClient;

        public RecreateSummariesLogService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>> GetAllRecreateSummariesLogsAsync(QueryParameters<string> query)
            => await _pactClient.PactRecreateSummariesLog.GetAllRecreateSummariesLogsAsync(query);
    }
}
