using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjectService : IProjectService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllPactProjectsAsync()
            => await _fpsClient.FpsProject.GetAllPactProjectsAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsAsync()
            => await _fpsClient.FpsProject.GetAllProjectsAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProject.GetPagedProjectsAsync(query);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProject.GetPagedPactProjectsAsync(query);

        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string parentProject)
            => await _fpsClient.FpsProject.GetProjectByIdAsync(parentProject);

        public async Task<ApiResponseDto<ProjectDto>> CreateProjectAsync(ProjectDto project)
            => await _fpsClient.FpsProject.CreateProjectAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdateProjectAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdatePactProjectAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactPortfolioAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdatePactPortfolioAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdateFpsPortfolioAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdateFpsPortfolioAsync(project);

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject)
            => await _fpsClient.FpsProject.DeleteProjectAsync(parentProject);

        public async Task<ApiResponseDto<List<StatusDto>>> GetAllStatusesAsync()
            => await _fpsClient.FpsLookup.GetAllStatusesAsync();

        public async Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync()
            => await _fpsClient.FpsLookup.GetAllDiseasesAsync();

        public async Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync()
            => await _fpsClient.FpsLookup.GetAllCustomersAsync();

        public async Task<ApiResponseDto<List<ContractDto>>> GetAllContractsAsync()
            => await _fpsClient.FpsLookup.GetAllContractsAsync();

        public async Task<ApiResponseDto<List<ContractDto>>> GetContractsByUserAsync()
            => await _fpsClient.FpsProject.GetContractsByUserAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo)
            => await _fpsClient.FpsProject.GetProjectsByProgramAsync(query, programNo);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup)
            => await _fpsClient.FpsProjectGroup.GetProjectsByProjectGroupAsync(query, projectGroup);

        public async Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync()
            => await _fpsClient.FpsLookup.GetAllProjectGroupsAsync();

        // Merged from ProgrammeNewProjectService
        public Task<ApiResponseDto<ProjectDto>> GetProgrammeNewProjectByIdAsync(string parentProject)
            => _fpsClient.FpsProject.GetProjectByIdAsync(parentProject);

        public Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string parentProject, ProjectDto project)
            => _fpsClient.FpsProject.UpdateProjectAsync(parentProject, project);

        public Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject)
            => _fpsClient.FpsProject.DeleteProjectAndChildrenAsync(parentProject);

        public Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode)
            => _fpsClient.FpsProject.ChangeProjectCodeAsync(oldCode, newCode);

        public Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code)
            => _fpsClient.FpsProject.CheckProjectExistsAsync(code);

        public Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync()
            => _fpsClient.FpsProject.GetManagersAsync();

        public Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync()
            => _fpsClient.FpsProject.GetCostCentresAsync();

        public Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync()
            => _fpsClient.FpsProjectGroup.GetAllProjectGroupsAsync();

        public Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsByUserAsync()
            => _fpsClient.FpsProjectGroup.GetProjectGroupsByUserAsync();

        public Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync()
            => _fpsClient.FpsProject.GetAccountCodesAsync();

        public Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync()
            => _fpsClient.FpsProject.GetSubAccountsAsync();

        public Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectProfitabilityAsync(
            QueryParameters<string> query, string programNo, string workTypeFilter)
            => _fpsClient.FpsProject.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);
    }
}
