using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyTimeRepository
    {
        Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject);

        Task<PagedData<MonthlyTimeLog>> SearchAsync(
            PaginationParameters<string> query,
            MonthlyTimeLogFilter monthlyTimeLogFilter);
    }
}
