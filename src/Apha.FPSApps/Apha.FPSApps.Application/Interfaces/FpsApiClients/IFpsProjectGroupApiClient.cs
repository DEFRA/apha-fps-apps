using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProjectGroupApiClient
    {
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsByUserAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup);
    }
}
