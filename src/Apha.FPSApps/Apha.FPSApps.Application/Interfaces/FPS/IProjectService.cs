// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IProjectService.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - Added GetProjectProfitabilityVlaAsync() — new frontend service interface method
 *     mirroring IFpsProjectApiClient.GetProjectProfitabilityVlaAsync() exactly.
 *   - Return type is List<ProjectProfitabilityVlaDto> (VLA-specific frontend DTO from Phase 7).
 *   - Four optional nullable filter params (projectStatus, programNo, manager, customer)
 *     match the backend GET /api/v1/project/profitability-vla query-string contract.
 *   - Pagination carried via QueryParameters<string> query.
 *
 * PRESERVED:
 *   - All 18 existing method signatures unchanged.
 *   - All using directives unchanged.
 *   - Namespace unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm MVC controller consumes GetProjectProfitabilityVlaAsync
 *     with all four filter params sourced from the VLA page filter dropdowns
 *     (filterProjectStatus, filterProgram, filterManager, filterCustomer).
 */

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
        Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string parentProject, ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdatePactPortfolioAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdateFpsPortfolioAsync(ProjectDto project);
        Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject);
        Task<ApiResponseDto<List<StatusDto>>> GetAllStatusesAsync();
        Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync();
        Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync();
        Task<ApiResponseDto<List<ContractDto>>> GetAllContractsAsync();
        Task<ApiResponseDto<List<ContractDto>>> GetContractsByUserAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup);

        // Merged from IProgrammeNewProjectService
        Task<ApiResponseDto<ProjectDto>> GetProgrammeNewProjectByIdAsync(string parentProject);
        Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject);
        Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode);
        Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code);
        Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync();
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsByUserAsync();
        Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync();
        Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync();
        Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectProfitabilityAsync(QueryParameters<string> query, string programNo, string workTypeFilter);
        Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectGroupProfitabilityAsync(QueryParameters<string> query, string projectGroup, string workTypeFilter);

        // TRANSFORMENGINE: new method — Phase 8 addition; mirrors IFpsProjectApiClient.GetProjectProfitabilityVlaAsync()
        // Delegates to backend GET /api/v1/project/profitability-vla via IFpsProjectApiClient.
        // All four filter params are optional; sourced from VLA page filter dropdowns.
        Task<ApiResponseDto<List<ProjectProfitabilityVlaDto>>> GetProjectProfitabilityVlaAsync(
            QueryParameters<string> query,
            string? projectStatus = null,
            string? programNo = null,
            string? manager = null,
            string? customer = null);
    }
}
