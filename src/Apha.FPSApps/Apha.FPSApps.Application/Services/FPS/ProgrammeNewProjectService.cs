using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProgrammeNewProjectService : IProgrammeNewProjectService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProgrammeNewProjectService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public Task<ApiResponseDto<ProgrammeNewProjectDto>> GetProjectByIdAsync(string parentProject)
            => _fpsClient.FpsProgrammeNewProject.GetProjectByIdAsync(parentProject);

        public Task<ApiResponseDto<ProgrammeNewProjectDto>> CreateProjectAsync(ProgrammeNewProjectDto project)
            => _fpsClient.FpsProgrammeNewProject.CreateProjectAsync(project);

        public Task<ApiResponseDto<ProgrammeNewProjectDto>> UpdateProjectAsync(string parentProject, ProgrammeNewProjectDto project)
            => _fpsClient.FpsProgrammeNewProject.UpdateProjectAsync(parentProject, project);

        public Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject)
            => _fpsClient.FpsProgrammeNewProject.DeleteProjectAndChildrenAsync(parentProject);

        public Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode)
            => _fpsClient.FpsProgrammeNewProject.ChangeProjectCodeAsync(oldCode, newCode);

        public Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code)
            => _fpsClient.FpsProgrammeNewProject.CheckProjectExistsAsync(code);

        public Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync()
            => _fpsClient.FpsProgrammeNewProject.GetManagersAsync();

        public Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync()
            => _fpsClient.FpsProgrammeNewProject.GetCostCentresAsync();

        public Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync()
            => _fpsClient.FpsProgrammeNewProject.GetProjectGroupsAsync();

        public Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync()
            => _fpsClient.FpsProgrammeNewProject.GetAccountCodesAsync();

        public Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync()
            => _fpsClient.FpsProgrammeNewProject.GetSubAccountsAsync();
    }
}
