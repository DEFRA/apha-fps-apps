/*
 * TRANSFORMENGINE MIGRATION — IProjectAuditTrailRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — no legacy equivalent (MS Access queried tables directly via DAO/ADODB)
 *   - Created repository interface for the ProjectAuditTrail resource family
 *   - 5 async query methods covering all 5 partitioned log tables in fps schema
 *   - All methods accept: project filter (ParentProject/JobCode), optional date range, and pagination parameters
 *   - Uses PagedData<T> + PaginationParameters<string> — consistent with existing IProjectRepository pattern
 *
 * PRESERVED:
 *   - Core layer constraint: no DbContext, EF mapping, or infrastructure-specific references
 *   - Async-only signatures matching project repository conventions
 *   - Nullable date parameters — date range is optional per JS prototype filter-from/filter-to inputs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetStaffJobLogsAsync filter — staffjob_log has no parentproject column; filter by JobCode prefix or join to tlkpjob is needed in repository impl
 *   - TRANSFORMENGINE TODO: FpsYear filter strategy — queries should accept fpsYear param or use active-year from context; noted for Phase 4 repository impl
 */
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectAuditTrailRepository
    {
        // TRANSFORMENGINE: GetProjectLogsAsync — queries fps.project_log filtered by ParentProject + optional date range
        Task<PagedData<ProjectLog>> GetProjectLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetStaffJobLogsAsync — queries fps.staffjob_log filtered by JobCode (derived from parentProject) + optional date range
        Task<PagedData<StaffJobLog>> GetStaffJobLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetTestRequirementLogsAsync — queries fps.testreq_log filtered by ProjectBuyerCode/JobCode + optional date range
        Task<PagedData<TestRequirementLog>> GetTestRequirementLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetAnimalRequestLogsAsync — queries fps.animalreq_log filtered by JobCode (derived from parentProject) + optional date range
        Task<PagedData<AnimalRequestLog>> GetAnimalRequestLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        // TRANSFORMENGINE: GetAdditionalCostLogsAsync — queries fps.additionalcosts_log filtered by JobCode (derived from parentProject) + optional date range
        Task<PagedData<AdditionalCostLog>> GetAdditionalCostLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
