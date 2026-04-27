using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITestorProductService
    {
        Task<PaginatedResult<TestorProductDto>> GetPagedTestOrProductsAsync(QueryParameters<string> query);
        Task<TestorProductDto?> GetTestorProductByIdAsync(string itemCode);
        Task<TestorProductDto> CreateTestorProductAsync(TestorProductDto dto);
        Task<TestorProductDto> UpdateTestorProductAsync(TestorProductDto dto);
        Task<bool> DeleteTestorProductAsync(string itemCode);
        Task<IEnumerable<string>> GetOwnersAsync();
    }
}
