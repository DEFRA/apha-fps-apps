using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITestSupplierRepository
    {
        Task<PagedData<TestSupplierView>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query,
            string testCode,
            bool showRejected);

        Task<TestRequirement?> GetByIdAsync(string testCode, string buyer);

        Task<TestRequirement> AddAsync(TestRequirement entity);

        Task<TestRequirement> UpdateAsync(TestRequirement entity);

        Task<bool> DeleteAsync(string testCode, string buyer);

        Task<List<TestOrProduct>> GetTestOrProductsAsync();

        Task<bool> ProjectExistsAsync(string parentProject);

        Task<bool> TestBuyerCodeExistsAsync(string testCode, string workGroup);

        Task<bool> MonthlyOutputExistsAsync(string testCode, string buyer);
    }
}
