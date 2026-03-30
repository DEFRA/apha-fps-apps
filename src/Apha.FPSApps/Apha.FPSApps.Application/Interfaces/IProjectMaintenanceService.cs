using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface IProjectMaintenanceService
    {
        Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string parentProject);
        Task<ApiResponseDto<ProjectDto>> CreateProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project);
        Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject);
        Task<ApiResponseDto<List<StatusDto>>> GetAllStatusesAsync();
        Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync();
        Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync();
        Task<ApiResponseDto<List<ContractDto>>> GetAllContractsAsync();
    }
}
