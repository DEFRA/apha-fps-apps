using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsJobCodeApiClient
    {
        Task<ApiResponseDto<IEnumerable<FpsJobCodeDto>>> GetZtJobCodesAsync();
    }
}
