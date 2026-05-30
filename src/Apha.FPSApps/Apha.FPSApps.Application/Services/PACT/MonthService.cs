using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class MonthService : IMonthService
    {
        private readonly IPactApiClient _pactClient;

        public MonthService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<MonthDto>>> GetAllMonthsAsync()
        {
            return await _pactClient.PactMonth.GetAllMonthsAsync();
        }
    }
}
