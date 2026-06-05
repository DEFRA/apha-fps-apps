using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthHourService
    {
        Task<PaginatedResult<MonthHourDto>> GetAllAsync(QueryParameters<string> query);
        Task<IEnumerable<MonthHourDto>> GetByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();
    }
}
