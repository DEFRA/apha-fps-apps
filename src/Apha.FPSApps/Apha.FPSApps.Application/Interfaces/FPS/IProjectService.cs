using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProjectService
    {
        Task<ApiResponseDto<List<ProjectDto>>> GetAllPactProjectsAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsAsync();
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
        Task<ApiResponseDto<List<ContractDto>>> GetContractsByUserAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync();

        // Merged from IProgrammeNewProjectService
        Task<ApiResponseDto<ProjectDto>> GetProgrammeNewProjectByIdAsync(string parentProject);
        Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string parentProject, ProjectDto project);
        Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject);
        Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode);
        Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code);
        Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync();
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsByUserAsync();
        Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync();
        Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync();
    }
}
