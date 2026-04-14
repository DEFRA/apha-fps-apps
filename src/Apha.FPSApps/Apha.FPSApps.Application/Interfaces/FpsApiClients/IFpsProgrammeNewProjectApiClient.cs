using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProgrammeNewProjectApiClient
    {
        Task<ApiResponseDto<ProgrammeNewProjectDto>> GetProjectByIdAsync(string parentProject);
        Task<ApiResponseDto<ProgrammeNewProjectDto>> CreateProjectAsync(ProgrammeNewProjectDto project);
        Task<ApiResponseDto<ProgrammeNewProjectDto>> UpdateProjectAsync(string parentProject, ProgrammeNewProjectDto project);
        Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject);
        Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode);
        Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code);
        Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync();
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync();
        Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync();
        Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync();
    }
}
