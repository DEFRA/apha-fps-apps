using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactTestListApiClient
    {
        Task<ApiResponseDto<List<TestOrProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<TestOrProductDto>> GetTestOrProductByIdAsync(string itemCode);
        Task<ApiResponseDto<TestOrProductDto>> CreateTestOrProductAsync(TestOrProductDto dto);
        Task<ApiResponseDto<TestOrProductDto>> UpdateTestOrProductAsync(string itemCode, TestOrProductDto dto);
        Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode);
        Task<ApiResponseDto<List<string>>> GetOwnersAsync();
    }
}

