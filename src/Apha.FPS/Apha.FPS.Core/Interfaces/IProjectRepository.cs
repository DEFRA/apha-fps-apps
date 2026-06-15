// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IProjectRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - Added GetProjectProfitabilityVlaAsync — new repository method for the
 *     Project Profitability VLA list (frmJobcodeTotalsVLA form migration).
 *   - Method signature: accepts ProjectProfitabilityVlaReq filter request and
 *     returns PagedData<ProjectProfitabilityVlaView>.
 *   - New entity type ProjectProfitabilityVlaView added to using imports.
 *
 * PRESERVED:
 *   - All existing method signatures unchanged (GetProjectProfitabilityAsync,
 *     GetProjectGroupProfitabilityAsync, GetAllProjectsAsync, CRUD operations,
 *     delete guard checks, program FK validation).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm PaginationParameters<ProjectProfitabilityVlaReq>
 *     vs. a flat parameter approach once the Application-layer service is implemented.
 *     The filter struct is passed via PaginationParameters<T>.Filter to align with
 *     the existing pattern used by GetProjectProfitabilityAsync.
 */

using Apha.Common.Contracts.FPS;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectRepository
    {
        // ProjectProfitability — project profitability query
        Task<PagedData<ProjectProfitabilityView>> GetProjectProfitabilityAsync(PaginationParameters<string> query, string programNo, string workTypeFilter);
        Task<PagedData<ProjectProfitabilityView>> GetProjectGroupProfitabilityAsync(PaginationParameters<string> query, string projectGroup, string workTypeFilter);

        // TRANSFORMENGINE: new method — VLA-filtered project profitability list
        //   Added for frmJobcodeTotalsVLA migration; filter dimensions: ProjectStatus,
        //   ProgramNo, Manager, Customer (from ProjectProfitabilityVlaReq).
        //   Returns paged ProjectProfitabilityVlaView rows from vprojectprofitabilityvla view.
        Task<PagedData<ProjectProfitabilityVlaView>> GetProjectProfitabilityVlaAsync(PaginationParameters<ProjectProfitabilityVlaReq> query);
        Task<IEnumerable<ProjectView>> GetAllProjectsAsync();
        Task<IEnumerable<PactProjectView>> GetAllPactProjectsAsync();
        Task<PagedData<Project>> GetPagedProjectsAsync(PaginationParameters<string> query);
        Task<PagedData<PactProjectView>> GetPagedPactProjectsAsync(PaginationParameters<string> query);
        Task<Project?> GetProjectByIdAsync(string parentProject);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<Project?> UpdatePactProjectDetailsAsync(Project project);
        Task<Project?> UpdatePactPortfolioDetailsAsync(Project project);
        Task<Project?> UpdateFpsPortfolioDetailsAsync(Project project);
        Task<bool> DeleteProjectAsync(string parentProject);
        Task<bool> HasAssociatedJobCodesAsync(string parentProject);
        Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo);
        Task<PagedData<Project>> GetProjectsByProjectGroupAsync(PaginationParameters<string> query, string projectGroup);

        // ProgrammeNewProject operations
        Task<bool> CheckProjectExistsAsync(string newProject);
        Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject);
        Task ChangeProjectCodeAsync(string oldCode, string newCode);
        Task DeleteProjectAndChildrenAsync(string parentProject);

        // Delete guard checks
        Task<bool> HasPlannedTestsAsync(string parentProject);
        Task<bool> HasMonthlyOutputAsync(string parentProject);
        Task<bool> HasMonthlyTimeAsync(string parentProject);
        Task<bool> HasProjectInvoicesAsync(string parentProject);
        Task<bool> HasProjectSubcontractsAsync(string parentProject);

        // Program FK validation (derived from tI_tlkpProject / tU_tlkpProject triggers)
        Task<bool> CheckProgramExistsAsync(string programNo);
    }
}
