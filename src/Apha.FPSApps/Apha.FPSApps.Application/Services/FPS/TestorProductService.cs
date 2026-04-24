using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class TestorProductService : ITestorProductService
    {
        private readonly IFpsApiClient _fpsClient;

        public TestorProductService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync()
            => await _fpsClient.FpsTestorProduct.GetAllTestorProductsAsync();
    }
}
