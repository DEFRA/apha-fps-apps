using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class MonthHourService : IMonthHourService
    {
        private readonly IPactApiClient _pactApiClient;

        public MonthHourService(IPactApiClient pactApiClient)
        {
            _pactApiClient = pactApiClient;
        }

        public async Task<ApiResponseDto<List<MonthHourDto>>> GetAllAsync(QueryParameters<string> query)
            => await _pactApiClient.PactMonthHour.GetAllAsync(query);

        public async Task<ApiResponseDto<List<MonthHourDto>>> GetByYearAsync(short year)
            => await _pactApiClient.PactMonthHour.GetByYearAsync(year);

        public async Task<ApiResponseDto<List<short>>> GetDistinctYearsAsync()
            => await _pactApiClient.PactMonthHour.GetDistinctYearsAsync();
    }
}
