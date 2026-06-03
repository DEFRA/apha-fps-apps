using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class TestSupplierService : ITestSupplierService
    {
        private readonly IFpsApiClient _fpsClient;

        public TestSupplierService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedAsync(
            QueryParameters<string> query,
            string testCode,
            bool showRejected)
        {
            return await _fpsClient.FpsTestSupplier.GetPagedAsync(query, testCode, showRejected);
        }

        public async Task<ApiResponseDto<TestSupplierViewDto>> GetViewByIdAsync(string testCode, string buyer)
        {
            return await _fpsClient.FpsTestSupplier.GetViewByIdAsync(testCode, buyer);
        }

        public async Task<ApiResponseDto<FpsTestRequirementDto>> GetByIdAsync(string testCode, string buyer)
        {
            return await _fpsClient.FpsTestSupplier.GetByIdAsync(testCode, buyer);
        }

        public async Task<ApiResponseDto<FpsTestRequirementDto>> CreateAsync(FpsTestRequirementDto dto)
        {
            return await _fpsClient.FpsTestSupplier.CreateAsync(dto);
        }

        public async Task<ApiResponseDto<FpsTestRequirementDto>> UpdateAsync(FpsTestRequirementDto dto)
        {
            return await _fpsClient.FpsTestSupplier.UpdateAsync(dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer)
        {
            return await _fpsClient.FpsTestSupplier.DeleteAsync(testCode, buyer);
        }
    }
}
