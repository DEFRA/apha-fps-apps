/*
 * TRANSFORMENGINE MIGRATION — IProjectAuditTrailService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — service interface for the ProjectAuditTrail resource family
 *   - 5 async methods covering all 5 partitioned log tables in fps schema
 *   - All methods accept: project filter (parentProject), optional date range, and pagination parameters
 *   - Returns PaginatedResult<TDto> consistent with existing service interface conventions (e.g. IProjectService, IAdditionalCostService)
 *
 * PRESERVED:
 *   - Application layer constraint: no repository or DbContext references
 *   - Async-only signatures matching project service conventions
 *   - Nullable date parameters — date range is optional per HTML prototype filter-from/filter-to inputs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetStaffJobLogsAsync — parentProject filter translates to JobCode-prefix or join in repository; no direct parentProject column in staffjob_log
 *   - TRANSFORMENGINE TODO: FpsYear filter may be added in a future iteration if multi-year audit is required
 */
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectAuditTrailService
    {
        // TRANSFORMENGINE: GetProjectLogsAsync — delegates to IProjectAuditTrailRepository.GetProjectLogsAsync; filters fps.project_log by ParentProject + optional date range
        Task<PaginatedResult<ProjectLogDto>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetStaffJobLogsAsync — delegates to IProjectAuditTrailRepository.GetStaffJobLogsAsync; filters fps.staffjob_log by JobCode (derived from parentProject) + optional date range
        Task<PaginatedResult<StaffJobLogDto>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetTestRequirementLogsAsync — delegates to IProjectAuditTrailRepository.GetTestRequirementLogsAsync; filters fps.testreq_log by ProjectBuyerCode/JobCode + optional date range
        Task<PaginatedResult<TestRequirementLogDto>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetAnimalRequestLogsAsync — delegates to IProjectAuditTrailRepository.GetAnimalRequestLogsAsync; filters fps.animalreq_log by JobCode (derived from parentProject) + optional date range
        Task<PaginatedResult<AnimalRequestLogDto>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetAdditionalCostLogsAsync — delegates to IProjectAuditTrailRepository.GetAdditionalCostLogsAsync; filters fps.additionalcosts_log by JobCode (derived from parentProject) + optional date range
        Task<PaginatedResult<AdditionalCostLogDto>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
