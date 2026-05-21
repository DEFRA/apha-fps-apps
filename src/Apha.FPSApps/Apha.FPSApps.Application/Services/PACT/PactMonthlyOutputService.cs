using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class PactMonthlyOutputService : IPactMonthlyOutputService
    {
        private readonly IPactApiClient _pactApiClient;

        public PactMonthlyOutputService(IPactApiClient pactApiClient)
        {
            _pactApiClient = pactApiClient;
        }

        public async Task<ApiResponseDto<List<MonthlyOutputLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyOutputLogFilterDto filter)
            => await _pactApiClient.PactMonthlyOutput.SearchAsync(query, filter);
    }
}
