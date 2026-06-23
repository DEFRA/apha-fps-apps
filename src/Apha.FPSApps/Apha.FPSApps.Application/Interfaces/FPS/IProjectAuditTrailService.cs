/*
 * TRANSFORMENGINE MIGRATION — IProjectAuditTrailService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend service interface for the Project Audit Trail feature
 *   - 5 async read-only methods mirroring IFpsProjectAuditTrailApiClient signatures exactly
 *   - All methods accept QueryParameters<string>, required string project, and optional DateOnly? date range
 *   - No CRUD operations — this is a pure audit log viewer with 5 tabbed log types
 *
 * PRESERVED:
 *   - Exact parameter semantics from backend controller: project required, fromDate/toDate optional
 *   - All 5 log endpoint methods: project logs, staff job logs, test requirement logs,
 *     animal request logs, additional cost logs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether DateOnly? parameters should remain DateOnly? or convert
 *     to DateTime? at the service boundary (tracked from Phase 7 API client)
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for Project Audit Trail.
    /// Thin delegate — all methods forward to IFpsProjectAuditTrailApiClient via IFpsApiClient.
    /// Binds to backend ProjectAuditTrailController at /api/v1/projectaudittrail.
    /// </summary>
    public interface IProjectAuditTrailService
    {
        // TRANSFORMENGINE: Tab 1 — Project Detail Changes; maps to GET /api/v1/projectaudittrail/projectlogs
        Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: Tab 2 — Staff Plan Changes; maps to GET /api/v1/projectaudittrail/staffjoblogs
        Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: Tab 3 — Test Requirement Changes; maps to GET /api/v1/projectaudittrail/testrequirementlogs
        Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: Tab 4 — Animal Requirement Changes; maps to GET /api/v1/projectaudittrail/animalrequestlogs
        Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: Tab 5 — Exceptional Cost Changes; maps to GET /api/v1/projectaudittrail/additionalcostlogs
        Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);
    }
}
