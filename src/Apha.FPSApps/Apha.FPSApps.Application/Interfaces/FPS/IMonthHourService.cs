using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IMonthHourService
    {
        Task<ApiResponseDto<List<MonthHourDto>>> GetAllAsync(QueryParameters<string> query);
        Task<ApiResponseDto<IEnumerable<MonthHourDto>>> GetByYearAsync(short year);
        Task<ApiResponseDto<IEnumerable<short>>> GetDistinctYearsAsync();
        Task<ApiResponseDto<List<YearEndMonthHourDto>>> GetYearEndMonthHoursAsync();
        Task<ApiResponseDto<MonthHourDto>> SaveAsync(MonthHourDto dto);
    }
}
