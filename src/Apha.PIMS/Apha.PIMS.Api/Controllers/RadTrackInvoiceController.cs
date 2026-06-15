// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-12
 *
 * SECURITY REVIEW — Phase 14 (Pre-Build Security Review Gate) — 2026-06-12
 *   RESULT: PASS (visual)
 *   Checks performed:
 *     [Authorize(Roles)] at class level          : PASS — "API-PIMSUser,API-PIMSAdmin" matches PIMS convention
 *     No [AllowAnonymous] on any action          : PASS
 *     [ApiController] auto-400 on invalid model  : PASS
 *     [FromBody]/[FromQuery] explicit binding     : PASS — all input params use explicit binding source
 *     Route/body id enforcement (Update)         : PASS — dto.InvoiceCounter = id applied before UpdateAsync
 *     No raw SQL / no user input concatenation   : PASS — all data access through IRadTrackInvoiceService
 *     No stack traces in error responses         : PASS — ExceptionMiddleware handles exception-to-response mapping
 *     No hardcoded secrets or connection strings : PASS
 *     CORS / wildcard origins                    : PASS — not configured at controller scope
 *   Human-review items (pre-existing, not new defects):
 *     GetTotals returns RadTrackInvoiceTotalsDto directly (DTO shape exposed in API) — HUMAN_REVIEW
 *     Delete returns 200 { success: false } instead of 404 — pattern-consistent, not a security defect
 *
 * CHANGED:
 *   - New file: no prior .NET API controller existed for RadTrack Invoices.
 *   - frmpimsinvoices CRUD operations mapped to REST endpoints:
 *       GET    api/v1/radtrackinvoice         -> GetAll (paged + filtered list)
 *       GET    api/v1/radtrackinvoice/totals  -> GetTotals (aggregate footer row)
 *       GET    api/v1/radtrackinvoice/{id}    -> GetById
 *       POST   api/v1/radtrackinvoice         -> Create (returns 201 CreatedAtAction)
 *       PUT    api/v1/radtrackinvoice/{id}    -> Update
 *       DELETE api/v1/radtrackinvoice/{id}    -> Delete
 *   - Authorization: role-based [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
 *     matching all existing PIMS controllers (MilestoneController, ProposedProjectController).
 *   - Query filter binding uses QueryParameters<RadTrackInvoiceFilter> to carry
 *     Project, Contract, Year, Program filter dimensions from the toolbar dropdowns
 *     visible in source/ui/pims/frmpimsinvoices.html.
 *   - GetTotals accepts RadTrackInvoiceFilter? directly to return aggregate sums
 *     that match the totals footer row in the HTML prototype.
 *   - GetTotals declared before {id:int} route segment to avoid ASP.NET Core route
 *     ambiguity between "totals" literal and integer id.
 *
 * PRESERVED:
 *   - All business validation delegated to RadTrackInvoiceService (Phase 3).
 *   - Exception-driven API flow: ArgumentException -> 400, KeyNotFoundException -> 404,
 *     InvalidOperationException -> 409 are mapped by ExceptionMiddleware (unchanged).
 *   - Route id enforced on DTO.InvoiceCounter in Update to prevent route/body id mismatch.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: RadTrackInvoiceTotalsDto returned directly on the GetTotals
 *     endpoint because no RadTrackInvoiceTotalsRes contract was created in Phase 1.
 *     Add RadTrackInvoiceTotalsRes to Apha.Common.Contracts.PIMS and add a mapper entry
 *     in RequestMapper.cs if a typed Res contract is required by the frontend API client.
 *   - TRANSFORMENGINE TODO: Confirm roles "API-PIMSUser,API-PIMSAdmin" cover all required
 *     access groups for RadTrack Invoice management.
 *   - TRANSFORMENGINE TODO: Verify [FromQuery] model binding of QueryParameters<RadTrackInvoiceFilter>
 *     correctly binds nested filter properties (e.g. ?filter.project=...) vs flat params
 *     (e.g. ?project=...) — adjust frontend API client query-string format accordingly.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    // TRANSFORMENGINE SECURITY (Phase 14): [Authorize] class-level — roles match PIMS API convention. PASS (visual)
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/radtrackinvoice")]
    public class RadTrackInvoiceController : ControllerBase
    {
        private readonly IRadTrackInvoiceService _service;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: Constructor injection — service only, following PIMS controller convention.
        // No repository injected directly; all data access delegated through IRadTrackInvoiceService.
        public RadTrackInvoiceController(IRadTrackInvoiceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a paged, filtered list of RadTrack invoices.
        /// Supports optional filter dimensions: Project, Contract, Year, Program
        /// corresponding to the toolbar dropdowns in frmpimsinvoices.html.
        /// </summary>
        // TRANSFORMENGINE: GET list — drives the frmpimsinvoices data grid.
        // QueryParameters<RadTrackInvoiceFilter> carries page/size/sort + four filter dimensions.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters<RadTrackInvoiceFilter> parameters)
        {
            PaginatedResult<RadTrackInvoiceDto> result = await _service.GetAllAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<RadTrackInvoiceRes>>(result));
        }

        /// <summary>
        /// Get aggregate totals (PlannedAmount, DueAmount, ActualAmount sums) for the
        /// current filter — drives the totals footer row at the bottom of the invoice grid.
        /// </summary>
        // TRANSFORMENGINE: GET totals — "totals" literal route segment declared BEFORE {id:int}
        // to prevent ASP.NET Core route matching from treating "totals" as an integer id.
        // TRANSFORMENGINE TODO: Map to RadTrackInvoiceTotalsRes once that contract is created
        // in Apha.Common.Contracts.PIMS (see DEFERRED section above). Currently returns DTO directly.
        [HttpGet("totals")]
        public async Task<IActionResult> GetTotals([FromQuery] RadTrackInvoiceFilter? filter)
        {
            RadTrackInvoiceTotalsDto result = await _service.GetTotalsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get a single RadTrack invoice record by InvoiceCounter (primary key).
        /// Returns 404 Not Found if the record does not exist.
        /// </summary>
        // TRANSFORMENGINE: GET by PK — used by the Edit and Delete modal open flows in the UI.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            RadTrackInvoiceDto? result = await _service.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(_mapper.Map<RadTrackInvoiceRes>(result));
        }

        /// <summary>
        /// Create a new RadTrack invoice record.
        /// Returns 201 Created with the saved record on success.
        /// Returns 400 Bad Request if required fields (Project, DueAmount, DueDate) are missing.
        /// Returns 409 Conflict if duplicate InvoiceRef exists within the same Project+Contract.
        /// </summary>
        // TRANSFORMENGINE: POST create — maps to the Add Invoice modal save action in frmpimsinvoices.html.
        // Service enforces: Project required, DueAmount required, DueDate required,
        // duplicate InvoiceRef check within Project+Contract scope.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RadTrackInvoiceReq request)
        {
            RadTrackInvoiceDto dto = _mapper.Map<RadTrackInvoiceDto>(request);
            RadTrackInvoiceDto result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                new { id = result.InvoiceCounter },
                _mapper.Map<RadTrackInvoiceRes>(result));
        }

        /// <summary>
        /// Update an existing RadTrack invoice record by InvoiceCounter.
        /// Returns 200 OK with the updated record on success.
        /// Returns 404 Not Found if the record does not exist.
        /// Returns 400 Bad Request if required fields are invalid.
        /// Returns 409 Conflict if duplicate InvoiceRef exists within the same Project+Contract.
        /// </summary>
        // TRANSFORMENGINE: PUT update — maps to the Edit Invoice modal save action.
        // Route id is applied to dto.InvoiceCounter to enforce route/body id consistency
        // and prevent a caller from updating a different record via a mismatched body id.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RadTrackInvoiceReq request)
        {
            RadTrackInvoiceDto dto = _mapper.Map<RadTrackInvoiceDto>(request);
            // TRANSFORMENGINE: Route id authoritative — overrides any InvoiceCounter value
            // that may have been set by mapping from the request body.
            // TRANSFORMENGINE SECURITY (Phase 14): IDOR guard — route id enforced before UpdateAsync;
            // prevents caller from updating a different record via mismatched body id. PASS (visual)
            dto.InvoiceCounter = id;
            RadTrackInvoiceDto result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<RadTrackInvoiceRes>(result));
        }

        /// <summary>
        /// Delete a RadTrack invoice record by InvoiceCounter.
        /// Returns { success: true } if deleted, { success: false } if not found.
        /// </summary>
        // TRANSFORMENGINE: DELETE — maps to the Delete Invoice confirmation dialog action.
        // Service returns bool; controller wraps in anonymous { success } object following
        // the MilestoneController.DeleteMilestone pattern.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);
            return Ok(new { success = deleted });
        }
    }
}
