using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsTestorProductApiClient
    {
        Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync();
    }
}
