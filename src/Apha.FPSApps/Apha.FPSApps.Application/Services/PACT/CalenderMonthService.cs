using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class CalenderMonthService : ICalenderMonthService
    {
        private readonly IPactApiClient _pactClient;

        public CalenderMonthService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<CalenderMonthDto>>> GetAllCalenderMonthsAsync()
        {
            return await _pactClient.PactCalenderMonth.GetAllCalenderMonthsAsync();
        }
    }
}
