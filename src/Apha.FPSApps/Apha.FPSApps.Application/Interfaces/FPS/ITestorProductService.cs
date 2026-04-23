using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface ITestorProductService
    {
        Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync();
    }
}
