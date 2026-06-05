using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthHourRepository
    {
        Task<PagedData<MonthHour>> GetAllAsync(PaginationParameters<string> query);
        Task<IEnumerable<MonthHour>> GetByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();
    }
}
