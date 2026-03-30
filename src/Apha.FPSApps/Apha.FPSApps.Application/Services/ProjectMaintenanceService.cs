using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class ProjectMaintenanceService : IProjectMaintenanceService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectMaintenanceService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

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
    }
}
