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

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>> GetAllRecreateSummariesLogsAsync(QueryParameters<string> query)
            => await _pactClient.PactRecreateSummariesLog.GetAllRecreateSummariesLogsAsync(query);
    }
}
