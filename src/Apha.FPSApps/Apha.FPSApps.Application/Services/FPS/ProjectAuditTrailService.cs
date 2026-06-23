/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend service implementation for the Project Audit Trail feature
 *   - Injects IFpsApiClient (aggregate); delegates all calls through _fpsClient.FpsProjectAuditTrail
 *   - 5 thin delegate methods — NO business logic, NO conditionals, NO data transformation
 *   - _fpsClient is private readonly (Sonar S2933 compliant)
 *
 * PRESERVED:
 *   - Exact parameter signatures from IProjectAuditTrailService / IFpsProjectAuditTrailApiClient
 *   - All 5 log method delegates: GetProjectLogsAsync, GetStaffJobLogsAsync,
 *     GetTestRequirementLogsAsync, GetAnimalRequestLogsAsync, GetAdditionalCostLogsAsync
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether DateOnly? parameters should remain DateOnly? or convert
 *     to DateTime? at the service boundary (tracked from Phase 7 API client)
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for Project Audit Trail.
    /// Thin delegate — every method forwards to IFpsApiClient.FpsProjectAuditTrail.
    /// Contains NO business logic; business logic lives exclusively in the backend service.
    /// </summary>
    public class ProjectAuditTrailService : IProjectAuditTrailService
    {
        // TRANSFORMENGINE: private readonly — Sonar S2933 compliant
        private readonly IFpsApiClient _fpsClient;

        public ProjectAuditTrailService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        // TRANSFORMENGINE: Tab 1 delegate — forwards to FpsProjectAuditTrail.GetProjectLogsAsync
        public async Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetProjectLogsAsync(query, project, fromDate, toDate);
        }

        // TRANSFORMENGINE: Tab 2 delegate — forwards to FpsProjectAuditTrail.GetStaffJobLogsAsync
        public async Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetStaffJobLogsAsync(query, project, fromDate, toDate);
        }

        // TRANSFORMENGINE: Tab 3 delegate — forwards to FpsProjectAuditTrail.GetTestRequirementLogsAsync
        public async Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetTestRequirementLogsAsync(query, project, fromDate, toDate);
        }

        // TRANSFORMENGINE: Tab 4 delegate — forwards to FpsProjectAuditTrail.GetAnimalRequestLogsAsync
        public async Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetAnimalRequestLogsAsync(query, project, fromDate, toDate);
        }

        // TRANSFORMENGINE: Tab 5 delegate — forwards to FpsProjectAuditTrail.GetAdditionalCostLogsAsync
        public async Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetAdditionalCostLogsAsync(query, project, fromDate, toDate);
        }
    }
}
