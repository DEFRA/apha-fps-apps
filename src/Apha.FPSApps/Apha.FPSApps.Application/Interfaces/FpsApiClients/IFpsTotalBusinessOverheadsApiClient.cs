using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsTotalBusinessOverheadsApiClient
    {
        Task<ApiResponseDto<TotalBusinessOverheadsDto>> GetAsync();
        Task<ApiResponseDto<TotalBusinessOverheadsDto>> UpdateAsync(TotalBusinessOverheadsDto dto);
    }
}
