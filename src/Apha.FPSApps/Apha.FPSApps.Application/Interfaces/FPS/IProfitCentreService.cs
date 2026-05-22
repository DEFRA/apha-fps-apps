using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProfitCentreService
    {
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync();
    }
}
