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

        Task<PaginatedResult<TestReqmtDto>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode);
        Task<IEnumerable<TestReqmtDto>> GetAllTestReqmtForExportAsync(string testCode, string? filterJson);
        Task<TestReqmtDto?> GetTestReqmtByIdAsync(string testCode, string buyer);
        Task<TestReqmtDto?> GetTestReqmtPricingAsync(string testCode, string? projectCode = null);
        Task<TestReqmtDto> AddTestReqmtAsync(TestReqmtDto dto);
        Task<TestReqmtDto> UpdateTestReqmtAsync(TestReqmtDto dto);
        Task<bool> DeleteTestReqmtAsync(string testCode, string buyer);

        Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync();
    }
}
