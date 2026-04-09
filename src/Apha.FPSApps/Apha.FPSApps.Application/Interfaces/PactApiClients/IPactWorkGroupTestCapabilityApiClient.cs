using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactWorkGroupTestCapabilityApiClient
    {
        Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup);
        Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode);
        Task<ApiResponseDto<TestCapabilityDto>> GetTestCapabilityByIdAsync(string testCode, string workGroup);
        Task<ApiResponseDto<TestCapabilityDto>> CreateTestCapabilityAsync(TestCapabilityDto dto);
        Task<ApiResponseDto<TestCapabilityDto>> UpdateTestCapabilityAsync(TestCapabilityDto dto);
        Task<ApiResponseDto<bool>> DeleteTestCapabilityAsync(string testCode, string workGroup);

        Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode);
        Task<ApiResponseDto<List<TestRequirementDto>>> GetAllTestReqmtForExportAsync(string testCode, string? filter);
        Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtByIdAsync(string testCode, string buyer);
        Task<ApiResponseDto<TestRequirementDto>> CreateTestReqmtAsync(TestRequirementDto dto);
        Task<ApiResponseDto<TestRequirementDto>> UpdateTestReqmtAsync(TestRequirementDto dto);
        Task<ApiResponseDto<bool>> DeleteTestReqmtAsync(string testCode, string buyer);

        Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync();
        Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtPricingAsync(string testCode, string? projectCode = null);
    }
}
