using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthlyTimeService
    {
        Task<PaginatedResult<MonthlyTimeLogDto>> SearchAsync(
                QueryParameters<string> query,
                MonthlyTimeLogFilterDto monthlyTimeLogFilter);
    }
}
