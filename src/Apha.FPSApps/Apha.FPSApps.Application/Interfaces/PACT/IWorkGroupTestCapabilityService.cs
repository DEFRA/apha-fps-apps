using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IWorkGroupTestCapabilityService
    {
        Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup);
        Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode);
        Task<ApiResponseDto<TestCapabilityDto>> GetTestCapabilityByIdAsync(string testCode, string workGroup);
        Task<ApiResponseDto<TestCapabilityDto>> CreateTestCapabilityAsync(TestCapabilityDto dto);
        Task<ApiResponseDto<TestCapabilityDto>> UpdateTestCapabilityAsync(TestCapabilityDto dto);
        Task<ApiResponseDto<bool>> DeleteTestCapabilityAsync(string testCode, string workGroup);

        Task<ApiResponseDto<List<TestReqmtDto>>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode);
        Task<ApiResponseDto<List<TestReqmtDto>>> GetAllTestReqmtForExportAsync(string testCode, string? filter);
        Task<ApiResponseDto<TestReqmtDto>> GetTestReqmtByIdAsync(string testCode, string buyer);
        Task<ApiResponseDto<TestReqmtDto>> CreateTestReqmtAsync(TestReqmtDto dto);
        Task<ApiResponseDto<TestReqmtDto>> UpdateTestReqmtAsync(TestReqmtDto dto);
        Task<ApiResponseDto<bool>> DeleteTestReqmtAsync(string testCode, string buyer);

        Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync();
        Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync();
        Task<ApiResponseDto<TestReqmtDto>> GetTestReqmtPricingAsync(string testCode, string? projectCode = null);
    }
}
