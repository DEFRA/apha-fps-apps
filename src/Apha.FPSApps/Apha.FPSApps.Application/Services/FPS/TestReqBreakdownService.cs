using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class TestReqBreakdownService : ITestReqBreakdownService
    {
        private readonly IFpsApiClient _fpsClient;

        public TestReqBreakdownService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<TestReqBreakdownDto>>> GetPlannedTestsByWorkgroupAsync(QueryParameters<string> query)
            => await _fpsClient.FpsTestReqBreakdown.GetPlannedTestsByWorkgroupAsync(query);
    }
}
