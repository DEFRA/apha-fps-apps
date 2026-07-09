using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface ITotalBusinessOverheadsService
    {
        Task<ApiResponseDto<TotalBusinessOverheadsDto>> GetAsync();
        Task<ApiResponseDto<TotalBusinessOverheadsDto>> UpdateAsync(TotalBusinessOverheadsDto dto);
    }
}
