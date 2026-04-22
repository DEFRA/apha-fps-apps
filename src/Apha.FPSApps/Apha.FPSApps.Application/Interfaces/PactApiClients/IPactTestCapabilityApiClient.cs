using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactTestCapabilityApiClient
    {
        Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup);
        Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode);
        Task<ApiResponseDto<TestCapabilityDto>> GetTestCapabilityByIdAsync(string testCode, string workGroup);
        Task<ApiResponseDto<TestCapabilityDto>> CreateTestCapabilityAsync(TestCapabilityDto dto);
        Task<ApiResponseDto<TestCapabilityDto>> UpdateTestCapabilityAsync(TestCapabilityDto dto);
        Task<ApiResponseDto<bool>> DeleteTestCapabilityAsync(string testCode, string workGroup);
    }
}
