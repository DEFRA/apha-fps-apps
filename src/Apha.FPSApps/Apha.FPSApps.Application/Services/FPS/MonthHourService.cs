using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class MonthHourService : IMonthHourService
    {
        private readonly IFpsApiClient _fpsClient;

        public MonthHourService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<MonthHourDto>>> GetAllAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsMonthHour.GetAllAsync(query);
        }

        public async Task<ApiResponseDto<IEnumerable<MonthHourDto>>> GetByYearAsync(short year)
        {
            return await _fpsClient.FpsMonthHour.GetByYearAsync(year);
        }

        public async Task<ApiResponseDto<IEnumerable<short>>> GetDistinctYearsAsync()
        {
            return await _fpsClient.FpsMonthHour.GetDistinctYearsAsync();
        }

        public async Task<ApiResponseDto<List<YearEndMonthHourDto>>> GetYearEndMonthHoursAsync()
        {
            return await _fpsClient.FpsMonthHour.GetYearEndMonthHoursAsync();
        }

        public async Task<ApiResponseDto<MonthHourDto>> SaveAsync(MonthHourDto dto)
        {
            return await _fpsClient.FpsMonthHour.SaveAsync(dto);
        }
    }
}
