using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestOrProductRepository
    {
        Task<PagedData<TestOrProduct>> GetPagedTestOrProductsAsync(PaginationParameters<string> parameters);
        Task<TestOrProduct?> GetTestOrProductByIdAsync(string itemCode);
        Task<TestOrProduct> CreateTestOrProductAsync(TestOrProduct entity);
        Task<TestOrProduct> UpdateTestOrProductAsync(TestOrProduct entity);
        Task<bool> DeleteTestOrProductAsync(string itemCode);
        Task<IEnumerable<string>> GetOwnersAsync();
    }
}
