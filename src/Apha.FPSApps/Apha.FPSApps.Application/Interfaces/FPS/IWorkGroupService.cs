using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupService
    {
        Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync();
        Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre);
    }
}
