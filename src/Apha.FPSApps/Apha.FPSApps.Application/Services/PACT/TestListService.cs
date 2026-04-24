using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class TestListService : ITestListService
    {
        private readonly IPactApiClient _apiClient;

        public TestListService(IPactApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query)
            => await _apiClient.PactTestList.GetPagedTestOrProductsAsync(query);

        public async Task<ApiResponseDto<TestorProductDto>> GetTestOrProductByIdAsync(string itemCode)
            => await _apiClient.PactTestList.GetTestOrProductByIdAsync(itemCode);

        public async Task<ApiResponseDto<TestorProductDto>> CreateTestOrProductAsync(TestorProductDto dto)
            => await _apiClient.PactTestList.CreateTestOrProductAsync(dto);

        public async Task<ApiResponseDto<TestorProductDto>> UpdateTestOrProductAsync(string itemCode, TestorProductDto dto)
            => await _apiClient.PactTestList.UpdateTestOrProductAsync(itemCode, dto);

        public async Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode)
            => await _apiClient.PactTestList.DeleteTestOrProductAsync(itemCode);

        public async Task<ApiResponseDto<List<string>>> GetOwnersAsync()
            => await _apiClient.PactTestList.GetOwnersAsync();
    }
}

