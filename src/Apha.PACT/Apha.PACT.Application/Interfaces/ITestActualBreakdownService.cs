using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITestActualBreakdownService
    {
        Task<PaginatedResult<TestActualBreakdownDto>> GetPagedAsync(QueryParameters<string> query);
    }
}
