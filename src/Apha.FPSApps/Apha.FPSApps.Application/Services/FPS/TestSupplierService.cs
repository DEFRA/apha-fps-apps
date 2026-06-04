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

        public async Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedTestSupplierAsync(
            QueryParameters<string> query, string testCode, bool showRejected)
            => await _fpsClient.FpsTestSupplier.GetPagedTestSupplierAsync(query, testCode, showRejected);
    }
}
