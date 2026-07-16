using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestActualBreakdownRepository
    {
        Task<PagedData<TestActualBreakdownView>> GetPagedAsync(PaginationParameters<string> query);
    }
}
