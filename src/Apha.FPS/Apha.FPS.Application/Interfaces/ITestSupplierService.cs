using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITestSupplierService
    {
        Task<PaginatedResult<TestSupplierViewDto>> GetPagedAsync(
            QueryParameters<string> query, string testCode, bool showRejected);
    }
}
