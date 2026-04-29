using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestorProductRepository
    {
        Task<IEnumerable<TestorProduct>> GetAllTestorProductsAsync();
        Task<PagedData<TestorProduct>> GetPagedTestOrProductsAsync(PaginationParameters<string> parameters);
        Task<TestorProduct?> GetTestOrProductByIdAsync(string itemCode);
        Task<TestorProduct> CreateTestOrProductAsync(TestorProduct entity);
        Task<TestorProduct> UpdateTestOrProductAsync(TestorProduct entity);
        Task<bool> DeleteTestOrProductAsync(string itemCode);
        Task<IEnumerable<string>> GetOwnersAsync();
        Task<Dictionary<string, string?>> GetDescriptionsByCodesAsync(IEnumerable<string> itemCodes);
    }
}
