/*
 * TRANSFORMENGINE MIGRATION — CostCentreController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI
 *            Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - Single GET endpoint (IStoredProcRepository workgroup lookup) → full CRUD API + workgroup lookup retained
 *   - Added ICostCentreService injection alongside IStoredProcRepository for the workgroup endpoint
 *   - Added IFpsRequestContext injection to supply FpsYear for composite-key service calls
 *   - frmMaintCostCentres DataGrid → GET api/v1/costcentre/paged (GetAllCostCentresPagedAsync)
 *   - frmMaintCostCentres Edit modal lookup → GET api/v1/costcentre/{costCentreNo} (GetCostCentreByIdAsync)
 *   - frmMaintCostCentres saveTblCostCentre() → POST api/v1/costcentre (CreateCostCentreAsync)
 *   - frmMaintCostCentres updateTblCostCentre() → PUT api/v1/costcentre/{costCentreNo} (UpdateCostCentreAsync)
 *   - frmMaintCostCentres handleTblCostCentreDelete() → DELETE api/v1/costcentre/{costCentreNo} (DeleteCostCentreAsync)
 *   - Existing GET api/v1/costcentre (GetAllCostCentresAsync workgroup lookup via stored proc) preserved
 *   - Phase 14 security review: no code changes required — all controls verified (see below)
 *
 * PRESERVED:
 *   - Route prefix api/v{version:apiVersion}/costcentre
 *   - Authorize roles: API-FPSUser, API-FPSAdmin, API-FPSShared
 *   - ApiVersion("1.0") and ControllerBase pattern
 *   - GetAllCostCentresAsync (stored-proc-based workgroup lookup) kept for existing consumers
 *
 * PHASE 14 SECURITY REVIEW RESULT — PASS:
 *   - [Authorize(Roles="API-FPSUser,API-FPSAdmin, API-FPSShared")] present at class level; all endpoints covered
 *   - [ApiController] enforces automatic 400 on model binding failures
 *   - FpsYear sourced server-side from IFpsRequestContext (X-FPS-Year header via middleware); never from client body
 *   - No raw SQL — all data access delegated through ICostCentreService and IStoredProcRepository
 *   - No hardcoded secrets, connection strings, or environment-specific values
 *   - Exception responses expose only client-supplied route param values (costCentreNo, FpsYear echo) — no stack traces
 *   - Route/body consistency: PUT route param (original key) vs body (new value) intentional to support key rename
 *   - No wildcard CORS, no [AllowAnonymous], no unsafe file/path handling
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: IStoredProcRepository is injected for workgroup lookup; if a dedicated service method is added to ICostCentreService for workgroup data, remove IStoredProcRepository from this controller.
 *   - TRANSFORMENGINE TODO: Verify composite key route param (double costCentreNo) is correctly round-tripped by the routing middleware for decimal values.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Cost Centre maintenance operations.
    /// Exposes paged list, single-record lookup, create, update, and delete endpoints
    /// derived from MS Access frmMaintCostCentres, plus a workgroup-lookup endpoint
    /// backed by the stored-procedure repository.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/costcentre")]
    public class CostCentreController : ControllerBase
    {
        // TRANSFORMENGINE: inject ICostCentreService for CRUD operations (service-only pattern)
        private readonly ICostCentreService _costCentreService;
        // TRANSFORMENGINE: IStoredProcRepository retained for existing workgroup-lookup endpoint (GetAllCostCentresAsync)
        private readonly IStoredProcRepository _storedProcRepository;
        // TRANSFORMENGINE: IFpsRequestContext injected to supply FpsYear for composite-key operations (year set by RequestContextMiddleware from X-FPS-Year header)
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;

        public CostCentreController(
            ICostCentreService costCentreService,
            IStoredProcRepository storedProcRepository,
            IFpsRequestContext fpsRequestContext,
            IMapper mapper)
        {
            _costCentreService = costCentreService ?? throw new ArgumentNullException(nameof(costCentreService));
            _storedProcRepository = storedProcRepository ?? throw new ArgumentNullException(nameof(storedProcRepository));
            _fpsRequestContext = fpsRequestContext ?? throw new ArgumentNullException(nameof(fpsRequestContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // ─── Workgroup Lookup (retained from original implementation) ──────────────

        /// <summary>
        /// Returns all cost centres with their associated work groups for lookup purposes.
        /// Backed by the stored-procedure repository (GetAllCostCentreWorkgroupAsync).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CostCentreWorkgroupRes>>> GetAllCostCentresAsync()
        {
            // TRANSFORMENGINE: preserved from original controller — stored-proc workgroup lookup for downstream consumers
            var costCentres = await _storedProcRepository.GetAllCostCentreWorkgroupAsync();
            return Ok(_mapper.Map<IEnumerable<CostCentreWorkgroupRes>>(costCentres));
        }

        // ─── CRUD Endpoints (frmMaintCostCentres migration) ───────────────────────

        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of cost centres for the active FPS year.
        /// Drives the DataGrid in fps_costcenter_maintenance.html (#gridContainer_costcenterList).
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paginated list of <see cref="CostCentreRes"/>.</returns>
        // TRANSFORMENGINE: GET paged — maps to frmMaintCostCentres DataGrid source; delegates to ICostCentreService.GetAllCostCentresPagedAsync
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllCostCentresPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _costCentreService.GetAllCostCentresPagedAsync(query);
            if (result == null)
                throw new ArgumentException("Cost centre records not found");

            return Ok(_mapper.Map<PaginationRes<CostCentreRes>>(result));
        }

        /// <summary>
        /// Returns a single cost centre by its cost centre number for the active FPS year.
        /// Populates the Edit modal fields (modal-cc-number, modal-cc-profit) in fps_costcenter_maintenance.html.
        /// </summary>
        /// <param name="costCentreNo">Cost centre number (double precision).</param>
        /// <returns><see cref="CostCentreRes"/> if found.</returns>
        // TRANSFORMENGINE: GET by composite key — maps to Edit modal load; FpsYear resolved from IFpsRequestContext (X-FPS-Year header via middleware)
        [HttpGet("{costCentreNo}")]
        public async Task<ActionResult<CostCentreRes>> GetCostCentreByIdAsync(double costCentreNo)
        {
            var dto = await _costCentreService.GetCostCentreByIdAsync(costCentreNo, _fpsRequestContext.FpsYear);
            if (dto == null)
                throw new ArgumentException($"Cost centre record '{costCentreNo}' not found for FPS year '{_fpsRequestContext.FpsYear}'");

            return Ok(_mapper.Map<CostCentreRes>(dto));
        }

        /// <summary>
        /// Creates a new cost centre record for the active FPS year.
        /// Maps to saveTblCostCentre() in costcenter_maintenance.js.
        /// </summary>
        /// <param name="request">Cost centre creation request (CostCentreNo + ProfitCentre).</param>
        /// <returns>Created <see cref="CostCentreRes"/>.</returns>
        // TRANSFORMENGINE: POST create — maps to saveTblCostCentre(); CostCentreReq → CostCentreDto; FpsYear injected from request context before passing to service
        [HttpPost]
        public async Task<ActionResult<CostCentreRes>> CreateCostCentreAsync([FromBody] CostCentreReq request)
        {
            var dto = _mapper.Map<CostCentreDto>(request);
            // TRANSFORMENGINE: FpsYear set server-side from IFpsRequestContext; client never supplies this value (excluded from CostCentreReq per contract design)
            dto.FpsYear = _fpsRequestContext.FpsYear;
            var created = await _costCentreService.CreateCostCentreAsync(dto);
            return Ok(_mapper.Map<CostCentreRes>(created));
        }

        /// <summary>
        /// Updates an existing cost centre record identified by its cost centre number in the active FPS year.
        /// Maps to updateTblCostCentre() in costcenter_maintenance.js.
        /// </summary>
        /// <param name="costCentreNo">Original cost centre number to identify the record.</param>
        /// <param name="request">Cost centre update request.</param>
        /// <returns>Updated <see cref="CostCentreRes"/>.</returns>
        // TRANSFORMENGINE: PUT update — maps to updateTblCostCentre(); originalCostCentreNo from route; FpsYear from IFpsRequestContext
        [HttpPut("{costCentreNo}")]
        public async Task<ActionResult<CostCentreRes>> UpdateCostCentreAsync(
            double costCentreNo,
            [FromBody] CostCentreReq request)
        {
            var dto = _mapper.Map<CostCentreDto>(request);
            // TRANSFORMENGINE: FpsYear set server-side; originalCostCentreNo from route aligns with composite PK (costcentre, fpsyear)
            dto.FpsYear = _fpsRequestContext.FpsYear;
            var updated = await _costCentreService.UpdateCostCentreAsync(costCentreNo, _fpsRequestContext.FpsYear, dto);
            return Ok(_mapper.Map<CostCentreRes>(updated));
        }

        /// <summary>
        /// Deletes the cost centre record with the given cost centre number in the active FPS year.
        /// Maps to handleTblCostCentreDelete() in costcenter_maintenance.js.
        /// </summary>
        /// <param name="costCentreNo">Cost centre number of the record to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        // TRANSFORMENGINE: DELETE — maps to handleTblCostCentreDelete(); composite key (costCentreNo, fpsYear) resolved from route + context
        [HttpDelete("{costCentreNo}")]
        public async Task<IActionResult> DeleteCostCentreAsync(double costCentreNo)
        {
            var deleted = await _costCentreService.DeleteCostCentreAsync(costCentreNo, _fpsRequestContext.FpsYear);
            if (!deleted)
                throw new ArgumentException($"Cost centre record '{costCentreNo}' for FPS year '{_fpsRequestContext.FpsYear}' not found for deletion");

            return Ok(true);
        }
    }
}
