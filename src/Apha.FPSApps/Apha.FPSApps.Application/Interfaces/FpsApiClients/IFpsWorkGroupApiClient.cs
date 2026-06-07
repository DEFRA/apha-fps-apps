using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWorkGroupApiClient
    {
        Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync();
        Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre);
    }
}
