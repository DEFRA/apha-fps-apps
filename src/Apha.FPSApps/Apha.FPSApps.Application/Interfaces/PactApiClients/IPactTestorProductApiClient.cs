using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactTestorProductApiClient
    {
        Task<ApiResponseDto<List<TestorProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<TestorProductDto>> GetTestOrProductByIdAsync(string itemCode);
        Task<ApiResponseDto<TestorProductDto>> CreateTestOrProductAsync(TestorProductDto dto);
        Task<ApiResponseDto<TestorProductDto>> UpdateTestOrProductAsync(string itemCode, TestorProductDto dto);
        Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode);
        Task<ApiResponseDto<List<string>>> GetOwnersAsync();
    }
}

