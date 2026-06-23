/*
 * TRANSFORMENGINE MIGRATION — WorkgroupController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-23
 * Phase 6 verified  : 2026-06-23
 * Phase 14 security : 2026-06-23 — PASS (see Security Review section in transform-review-checklist.md)
 *
 * CHANGED:
 *   - New [ApiController] created; no prior WorkgroupController existed in this codebase
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP → fps.workgroup table)
 *   - All CRUD operations mapped to versioned REST routes under api/v1/workgroup:
 *       GET    api/v1/workgroup/paged          → GetPagedAsync      (paged grid list)
 *       GET    api/v1/workgroup/{workGroupName} → GetByKeyAsync      (single record fetch for edit modal)
 *       POST   api/v1/workgroup               → CreateAsync        (add new workgroup)
 *       PUT    api/v1/workgroup/{workGroupName} → UpdateAsync        (save edit; PK rename supported)
 *       DELETE api/v1/workgroup/{workGroupName} → DeleteAsync        (remove workgroup row)
 *   - Three lookup endpoints added for modal dropdown population (SEPARATE from CRUD resource family):
 *       GET    api/v1/workgroup/profitcentres  → GetProfitCentresAsync   (ResourceCentre select)
 *       GET    api/v1/workgroup/owners         → GetOwnersAsync          (Owner select, from qryManager)
 *       GET    api/v1/workgroup/costcentres    → GetCostCentresAsync     (cascading CostCentre select)
 *   - WorkgroupMaintenanceReq → WorkgroupDto via IMapper; WorkgroupDto → WorkgroupMaintenanceRes via IMapper
 *   - Exception-driven flow: throws ArgumentException / KeyNotFoundException; ExceptionMiddleware maps status codes
 *   - [Authorize] applied with FPS role set consistent with all other FPS API controllers
 *
 * PHASE 6 GATE — ROUTE + CONTRACT + MAPPER CONFIRMATION:
 *   - Routes confirmed against FpsApiEndpoints constants (GetPagedWorkgroups, GetWorkgroupByName,
 *     CreateWorkgroup, UpdateWorkgroup, DeleteWorkgroup, GetWorkgroupProfitCentres,
 *     GetWorkgroupOwners, GetWorkgroupCostCentres) — added to FpsApiEndpoints.cs Phase 6
 *   - CRUD routes verified: all 5 CRUD actions match transform-plan handoff notes exactly
 *   - Lookup routes verified: 3 lookup endpoints confirmed SEPARATE from main CRUD resource family
 *   - Required action parameters confirmed:
 *       {workGroupName} (GET/PUT/DELETE) — required business context (PK component); sourced from
 *         grid row selection / route state in the frontend
 *       profitCentre (GET costcentres) — required business filter; sourced from modal dropdown
 *         ProfitCentre selection change event
 *   - WorkgroupMaintenanceReq, WorkgroupMaintenanceRes, WorkgroupDto, RequestMapper all verified
 *   - ManagerRes (owners lookup) and IEnumerable<double?> (costcentres lookup) parameter shapes noted
 *
 * PHASE 14 SECURITY REVIEW RESULTS:
 *   - [Authorize] role set: "API-FPSUser,API-FPSAdmin, API-FPSShared" — PASS (consistent with all
 *     30+ other FPS API controllers; codebase-wide convention)
 *   - Input validation guards (null/empty checks): PASS — present on GetByKeyAsync, UpdateAsync,
 *     DeleteAsync, GetCostCentresAsync
 *   - Model-state validation: PASS — [ApiController] auto-validates CreateAsync and UpdateAsync
 *   - Exception disclosure: PASS — ArgumentException/KeyNotFoundException delegated to ExceptionMiddleware;
 *     no stack traces or connection strings in responses
 *   - Anti-forgery: N/A for [ApiController] API-only endpoints
 *   - CORS: PASS — no per-controller override; inherits global policy from Program.cs
 *   - Secrets: PASS — no hardcoded credentials, tokens, or connection strings
 *   - Raw SQL: PASS — all data access via IWorkgroupService (LINQ); no concatenated SQL
 *   - FpsYear: PASS — resolved server-side via FpsRequestContext query filter; not in request body
 *
 * PRESERVED:
 *   - Service-only injection (no repository injected directly into controller)
 *   - Async-only action signatures consistent with GradeController, DivisionController, etc.
 *   - XML summary doc on every public action
 *   - Route casing: lowercase "workgroup" matching the FpsApiEndpoints constants convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm [Authorize] role set matches the target environment's
 *     FPS API role configuration (currently mirrors GradeController/DivisionController);
 *     note: the space before "API-FPSShared" in the comma-delimited string is a codebase-wide
 *     convention — verify ASP.NET Core role-splitting trims whitespace in the target runtime
 *   - TRANSFORMENGINE TODO: GetCostCentresAsync returns IEnumerable<double?> — if the frontend
 *     needs a labelled projection (value + display text), update service + response type
 *   - TRANSFORMENGINE TODO: GetOwnersAsync returns ManagerRes — confirm qryManager result set
 *     is equivalent to the existing EmployeeController /managers endpoint before deciding whether
 *     the two can be merged
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// REST API controller for WorkGroup maintenance operations.
    /// Migrated from <c>frmMaintWorkGroup2</c> (RecordSource: WorkGroup_MAP → fps.workgroup).
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/workgroup")]
    [ApiController]
    [ApiVersion("1.0")]
    public class WorkgroupController : ControllerBase
    {
        private readonly IWorkgroupService _workgroupService;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: constructor — inject IWorkgroupService only (no direct repository); matches GradeController pattern
        /// <summary>
        /// Initialises the <see cref="WorkgroupController"/> with its required dependencies.
        /// </summary>
        public WorkgroupController(IWorkgroupService workgroupService, IMapper mapper)
        {
            _workgroupService = workgroupService ?? throw new ArgumentNullException(nameof(workgroupService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET paged — frmMaintWorkGroup2 grid list; delegates to IWorkgroupService.GetPagedAsync
        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of workgroups for the active FPS year.
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paginated list of <see cref="WorkgroupMaintenanceRes"/>.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult<PaginationRes<WorkgroupMaintenanceRes>>> GetPagedAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var result = await _workgroupService.GetPagedAsync(query);
            if (result == null)
            {
                throw new ArgumentException("Workgroup records not found.");
            }
            return Ok(_mapper.Map<PaginationRes<WorkgroupMaintenanceRes>>(result));
        }

        // TRANSFORMENGINE: GET by key — single workgroup load for the edit modal; delegates to IWorkgroupService.GetByKeyAsync
        /// <summary>
        /// Returns a single workgroup by its WorkGroupName for the active FPS year.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name (natural primary key component).</param>
        /// <returns><see cref="WorkgroupMaintenanceRes"/> if found.</returns>
        [HttpGet("{workGroupName}")]
        public async Task<ActionResult<WorkgroupMaintenanceRes>> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var dto = await _workgroupService.GetByKeyAsync(workGroupName);
            if (dto == null)
            {
                throw new KeyNotFoundException($"Workgroup '{workGroupName}' not found.");
            }
            return Ok(_mapper.Map<WorkgroupMaintenanceRes>(dto));
        }

        // TRANSFORMENGINE: POST create — frmMaintWorkGroup2 add-new path; WorkgroupMaintenanceReq → WorkgroupDto → entity
        /// <summary>
        /// Creates a new workgroup record.
        /// Throws <see cref="ArgumentException"/> if WorkGroupName or ProfitCentre is missing.
        /// Throws <see cref="InvalidOperationException"/> if a workgroup with the same name already exists in the active FPS year.
        /// </summary>
        /// <param name="request">Workgroup creation request.</param>
        /// <returns>Created <see cref="WorkgroupMaintenanceRes"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<WorkgroupMaintenanceRes>> CreateAsync([FromBody] WorkgroupMaintenanceReq request)
        {
            var dto = _mapper.Map<WorkgroupDto>(request);
            var created = await _workgroupService.CreateAsync(dto);
            return Ok(_mapper.Map<WorkgroupMaintenanceRes>(created));
        }

        // TRANSFORMENGINE: PUT update — frmMaintWorkGroup2 save-edit path; workGroupName route param is the original key
        //   supports PK rename: if request.WorkGroupName differs from {workGroupName}, the service/repository renames the record
        /// <summary>
        /// Updates an existing workgroup identified by <paramref name="workGroupName"/>.
        /// Pass the original WorkGroupName in the route; use <c>request.WorkGroupName</c> to rename.
        /// Throws <see cref="KeyNotFoundException"/> if the workgroup does not exist.
        /// </summary>
        /// <param name="workGroupName">Original WorkGroup name (route parameter).</param>
        /// <param name="request">Workgroup update request.</param>
        /// <returns>Updated <see cref="WorkgroupMaintenanceRes"/>.</returns>
        [HttpPut("{workGroupName}")]
        public async Task<ActionResult<WorkgroupMaintenanceRes>> UpdateAsync(
            string workGroupName,
            [FromBody] WorkgroupMaintenanceReq request)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var dto = _mapper.Map<WorkgroupDto>(request);
            var updated = await _workgroupService.UpdateAsync(workGroupName, dto);
            return Ok(_mapper.Map<WorkgroupMaintenanceRes>(updated));
        }

        // TRANSFORMENGINE: DELETE — frmMaintWorkGroup2 delete-row path; delegates to IWorkgroupService.DeleteAsync
        /// <summary>
        /// Deletes the workgroup with the given WorkGroupName in the active FPS year.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name of the record to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        [HttpDelete("{workGroupName}")]
        public async Task<IActionResult> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var deleted = await _workgroupService.DeleteAsync(workGroupName);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Workgroup '{workGroupName}' not found.");
            }
            return Ok(true);
        }

        // TRANSFORMENGINE: GET profitcentres — ResourceCentre dropdown in the add/edit modal;
        //   returns distinct ProfitCentreId values from fps.tblkpprofitcentre (not year-filtered)
        /// <summary>
        /// Returns all available profit centre identifiers for the ResourceCentre dropdown.
        /// </summary>
        /// <returns>List of profit centre identifier strings.</returns>
        [HttpGet("profitcentres")]
        public async Task<ActionResult<IEnumerable<string>>> GetProfitCentresAsync()
        {
            var result = await _workgroupService.GetAllProfitCentresAsync();
            return Ok(result);
        }

        // TRANSFORMENGINE: GET owners — Owner dropdown in the add/edit modal;
        //   maps to qryManager named query result (Manager entity → ManagerDto → ManagerRes via IMapper)
        /// <summary>
        /// Returns all manager records for the Owner dropdown.
        /// Sourced from the fps/qryManager named query (vtblstaffactive JOIN vworkgroupgrade_general).
        /// </summary>
        /// <returns>List of <see cref="ManagerRes"/> records.</returns>
        [HttpGet("owners")]
        public async Task<ActionResult<IEnumerable<ManagerRes>>> GetOwnersAsync()
        {
            var managerDtos = await _workgroupService.GetOwnersAsync();
            return Ok(_mapper.Map<IEnumerable<ManagerRes>>(managerDtos));
        }

        // TRANSFORMENGINE: GET costcentres — cascading CostCentre dropdown in the add/edit modal;
        //   triggered when ProfitCentre selection changes (VBA Form_Current: Requery CostCentre combo)
        //   returns distinct CostCentre (double?) values for the given profitCentre
        /// <summary>
        /// Returns cost centre values linked to the given <paramref name="profitCentre"/>,
        /// for use in the cascading CostCentre dropdown.
        /// </summary>
        /// <param name="profitCentre">The selected profit centre code.</param>
        /// <returns>List of cost centre double values.</returns>
        [HttpGet("costcentres")]
        public async Task<ActionResult<IEnumerable<double?>>> GetCostCentresAsync([FromQuery] string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new ArgumentException("ProfitCentre cannot be null or empty.", nameof(profitCentre));
            }

            var result = await _workgroupService.GetCostCentresByProfitCentreAsync(profitCentre);
            return Ok(result);
        }
    }
}
