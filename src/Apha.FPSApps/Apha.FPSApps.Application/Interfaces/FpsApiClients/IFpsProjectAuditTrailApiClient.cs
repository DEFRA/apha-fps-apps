/*
 * TRANSFORMENGINE MIGRATION — IFpsProjectAuditTrailApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — typed HTTP API client interface for the ProjectAuditTrail feature
 *   - 5 async methods matching backend ProjectAuditTrailController endpoints at
 *     GET /api/v1/projectaudittrail/{projectlogs|staffjoblogs|testrequirementlogs|animalrequestlogs|additionalcostlogs}
 *   - Each method takes: QueryParameters<string> for pagination, string project (required), DateOnly? fromDate and toDate (optional)
 *   - All return types wrapped in ApiResponseDto<List<T>> as per frontend envelope convention
 *
 * PRESERVED:
 *   - Exact parameter semantics from backend controller: project required, date range optional
 *   - All 5 log endpoint methods present: project logs, staff job logs, test requirement logs,
 *     animal request logs, additional cost logs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether DateOnly? params should remain DateOnly? or convert to DateTime? at client boundary
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Typed HTTP client interface for the Project Audit Trail feature.
    /// Binds to backend ProjectAuditTrailController at route /api/v1/projectaudittrail.
    /// </summary>
    public interface IFpsProjectAuditTrailApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/projectlogs — Tab 1 (Project Detail Changes)
        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/staffjoblogs — Tab 2 (Staff Plan Changes)
        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/testrequirementlogs — Tab 3 (Test Requirement Changes)
        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/animalrequestlogs — Tab 4 (Animal Requirement Changes)
        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/additionalcostlogs — Tab 5 (Exceptional Cost Changes)
        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);
    }
}
