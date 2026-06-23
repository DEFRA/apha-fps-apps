/*
 * TRANSFORMENGINE MIGRATION — IFpsWorkgroupApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: API client interface for WorkGroup Maintenance CRUD + lookup operations
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP → fps.workgroup)
 *   - CRUD methods mirror the 5 backend controller actions under api/v1/workgroup:
 *       GetPagedAsync           → GET  api/v1/workgroup/paged
 *       GetByWorkGroupNameAsync → GET  api/v1/workgroup/{workGroupName}
 *       CreateAsync             → POST api/v1/workgroup
 *       UpdateAsync             → PUT  api/v1/workgroup/{workGroupName}
 *       DeleteAsync             → DELETE api/v1/workgroup/{workGroupName}
 *   - Three lookup methods mirror the three dedicated lookup endpoints (SEPARATE from CRUD):
 *       GetProfitCentresAsync   → GET api/v1/workgroup/profitcentres  (ResourceCentre dropdown)
 *       GetOwnersAsync          → GET api/v1/workgroup/owners         (Owner dropdown, ManagerDto)
 *       GetCostCentresAsync     → GET api/v1/workgroup/costcentres?profitCentre={pc}
 *                                     (cascading CostCentre dropdown; profitCentre sourced from
 *                                      modal ProfitCentre selection — confirmed page-sourced parameter)
 *   - WorkGroupName route parameter (GET/PUT/DELETE) sourced from grid row selection / route state
 *   - profitCentre query parameter (GET costcentres) sourced from modal ProfitCentre change event
 *
 * PRESERVED:
 *   - Method naming convention consistent with IFpsWorkGroupGradeApiClient, IFpsStaffJobApiClient, etc.
 *   - All return types wrapped in ApiResponseDto<T> — standard FPSApps response envelope
 *   - Lookup methods return dedicated types (List<string>, List<ManagerDto>, List<double?>) matching
 *     backend controller return types — NOT reusing WorkgroupMaintenanceDto for lookups
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetCostCentresAsync returns List<double?> — if the frontend needs a
 *     labelled projection (value + display text), coordinate with backend to update the response type
 *   - TRANSFORMENGINE TODO: Verify GET paged response envelope — backend returns
 *     PaginationRes<WorkgroupMaintenanceRes>; infrastructure impl must map to ApiResponseDto<List<WorkgroupMaintenanceDto>>
 *     (FpsApiDtoMapper handles WorkgroupMaintenanceRes → WorkgroupMaintenanceDto in Phase 10)
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Typed HTTP API client interface for WorkGroup maintenance CRUD and lookup operations.
    /// Targets backend routes under <c>api/v1/workgroup</c> (WorkgroupController).
    /// Migrated from <c>frmMaintWorkGroup2</c>.
    /// </summary>
    public interface IFpsWorkgroupApiClient
    {
        // ── CRUD ────────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: GetPagedAsync → GET api/v1/workgroup/paged (frmMaintWorkGroup2 grid list)
        /// <summary>
        /// Returns a paginated list of workgroup maintenance records.
        /// Calls <c>GET api/v1/workgroup/paged</c>.
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paged list of <see cref="WorkgroupMaintenanceDto"/>.</returns>
        Task<ApiResponseDto<List<WorkgroupMaintenanceDto>>> GetPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GetByWorkGroupNameAsync → GET api/v1/workgroup/{workGroupName}
        //   workGroupName sourced from grid row selection / route state (confirmed page-sourced)
        /// <summary>
        /// Returns a single workgroup record by its WorkGroupName.
        /// Calls <c>GET api/v1/workgroup/{workGroupName}</c>.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name (natural PK component) sourced from grid row.</param>
        /// <returns><see cref="WorkgroupMaintenanceDto"/> if found.</returns>
        Task<ApiResponseDto<WorkgroupMaintenanceDto>> GetByWorkGroupNameAsync(string workGroupName);

        // TRANSFORMENGINE: CreateAsync → POST api/v1/workgroup (frmMaintWorkGroup2 add-new path)
        /// <summary>
        /// Creates a new workgroup record.
        /// Calls <c>POST api/v1/workgroup</c>.
        /// </summary>
        /// <param name="dto">Workgroup data to create.</param>
        /// <returns>Created <see cref="WorkgroupMaintenanceDto"/>.</returns>
        Task<ApiResponseDto<WorkgroupMaintenanceDto>> CreateAsync(WorkgroupMaintenanceDto dto);

        // TRANSFORMENGINE: UpdateAsync → PUT api/v1/workgroup/{workGroupName}
        //   workGroupName is the ORIGINAL key (before any rename); dto.WorkGroupName may differ (rename)
        //   workGroupName sourced from grid row selection / route state (confirmed page-sourced)
        /// <summary>
        /// Updates an existing workgroup identified by <paramref name="workGroupName"/>.
        /// Pass the original WorkGroupName; <paramref name="dto"/>.WorkGroupName may differ to support rename.
        /// Calls <c>PUT api/v1/workgroup/{workGroupName}</c>.
        /// </summary>
        /// <param name="workGroupName">Original WorkGroup name (route parameter).</param>
        /// <param name="dto">Updated workgroup data.</param>
        /// <returns>Updated <see cref="WorkgroupMaintenanceDto"/>.</returns>
        Task<ApiResponseDto<WorkgroupMaintenanceDto>> UpdateAsync(string workGroupName, WorkgroupMaintenanceDto dto);

        // TRANSFORMENGINE: DeleteAsync → DELETE api/v1/workgroup/{workGroupName}
        //   workGroupName sourced from grid row selection / route state (confirmed page-sourced)
        /// <summary>
        /// Deletes the workgroup with the given WorkGroupName.
        /// Calls <c>DELETE api/v1/workgroup/{workGroupName}</c>.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name to delete, sourced from grid row.</param>
        /// <returns>True if deletion succeeded.</returns>
        Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName);

        // ── Lookup endpoints (SEPARATE from CRUD resource family) ────────────────

        // TRANSFORMENGINE: GetProfitCentresAsync → GET api/v1/workgroup/profitcentres
        //   Populates ResourceCentre dropdown in the add/edit modal
        /// <summary>
        /// Returns all available profit centre identifiers for the ResourceCentre dropdown.
        /// Calls <c>GET api/v1/workgroup/profitcentres</c>.
        /// </summary>
        /// <returns>List of profit centre identifier strings.</returns>
        Task<ApiResponseDto<List<string>>> GetProfitCentresAsync();

        // TRANSFORMENGINE: GetOwnersAsync → GET api/v1/workgroup/owners
        //   Populates Owner dropdown in the add/edit modal (qryManager source → ManagerRes → ManagerDto)
        /// <summary>
        /// Returns all manager records for the Owner dropdown.
        /// Calls <c>GET api/v1/workgroup/owners</c>.
        /// Sourced from the fps/qryManager named query.
        /// </summary>
        /// <returns>List of <see cref="ManagerDto"/> records.</returns>
        Task<ApiResponseDto<List<ManagerDto>>> GetOwnersAsync();

        // TRANSFORMENGINE: GetCostCentresAsync → GET api/v1/workgroup/costcentres?profitCentre={pc}
        //   Cascading CostCentre dropdown — profitCentre is sourced from the modal ProfitCentre change event
        //   (VBA Form_Current: Requery CostCentre combo equivalent)
        /// <summary>
        /// Returns cost centre values for the cascading CostCentre dropdown filtered by profit centre.
        /// Calls <c>GET api/v1/workgroup/costcentres?profitCentre={profitCentre}</c>.
        /// Triggered when the modal ProfitCentre selection changes.
        /// </summary>
        /// <param name="profitCentre">
        /// Selected profit centre code — sourced from the modal ProfitCentre dropdown selection.
        /// </param>
        /// <returns>List of cost centre double values for the given profit centre.</returns>
        Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre);
    }
}
