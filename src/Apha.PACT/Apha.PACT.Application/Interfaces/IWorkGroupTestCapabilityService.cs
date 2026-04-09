using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IWorkGroupTestCapabilityService
    {
        Task<PaginatedResult<TestCapabilityDto>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup);
        Task<PaginatedResult<TestCapabilityDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode);
        Task<TestCapabilityDto?> GetTestCapabilityByIdAsync(string testCode, string workGroup);
        Task<TestCapabilityDto> AddTestCapabilityAsync(TestCapabilityDto dto);
        Task<TestCapabilityDto> UpdateTestCapabilityAsync(TestCapabilityDto dto);
        Task<bool> DeleteTestCapabilityAsync(string testCode, string workGroup);

        Task<PaginatedResult<TestRequirementtDto>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode);
        Task<IEnumerable<TestRequirementtDto>> GetAllTestReqmtForExportAsync(string testCode, string? filterJson);
        Task<TestRequirementtDto?> GetTestReqmtByIdAsync(string testCode, string buyer);
        Task<TestRequirementtDto?> GetTestReqmtPricingAsync(string testCode, string? projectCode = null);
        Task<TestRequirementtDto> AddTestReqmtAsync(TestRequirementtDto dto);
        Task<TestRequirementtDto> UpdateTestReqmtAsync(TestRequirementtDto dto);
        Task<bool> DeleteTestReqmtAsync(string testCode, string buyer);

        Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync();
    }
}
