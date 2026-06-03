using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITestSupplierService
    {
        Task<PaginatedResult<TestSupplierViewDto>> GetPagedByTestCodeAsync(
            QueryParameters<string> query,
            string testCode,
            bool showRejected);

        Task<TestRequirementDto?> GetByIdAsync(string testCode, string buyer);

        Task<TestRequirementDto> AddAsync(TestRequirementDto dto);

        Task<TestRequirementDto> UpdateAsync(TestRequirementDto dto);

        Task<bool> DeleteAsync(string testCode, string buyer);

        Task<List<TestOrProductDto>> GetTestOrProductsAsync();
    }
}
