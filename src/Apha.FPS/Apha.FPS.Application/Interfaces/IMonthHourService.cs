using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMonthHourService
    {
        Task<PaginatedResult<MonthHourDto>> GetAllAsync(QueryParameters<string> query);
        Task<IEnumerable<MonthHourDto>> GetByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();
        Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync();
        Task<MonthHourDto> SaveAsync(MonthHourDto dto);
    }
}
