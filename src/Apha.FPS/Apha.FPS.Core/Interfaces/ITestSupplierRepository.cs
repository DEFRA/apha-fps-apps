using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITestSupplierRepository
    {
        Task<PagedData<TestSupplierView>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode, bool showRejected);
    }
}
