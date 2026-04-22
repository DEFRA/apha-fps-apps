using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITestListService
    {
        Task<PaginatedResult<TestOrProductDto>> GetPagedTestOrProductsAsync(QueryParameters<string> query);
        Task<TestOrProductDto?> GetTestOrProductByIdAsync(string itemCode);
        Task<TestOrProductDto> CreateTestOrProductAsync(TestOrProductDto dto);
        Task<TestOrProductDto> UpdateTestOrProductAsync(TestOrProductDto dto);
        Task<bool> DeleteTestOrProductAsync(string itemCode);
        Task<IEnumerable<string>> GetOwnersAsync();
    }
}
