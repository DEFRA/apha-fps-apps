// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 *            Phase 6 — Backend Readiness Gate - Route + Contract + Mapper Confirmation
 *            Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added [HttpPost] CreateWorkGroupEmployeeAsync action to support Add/Create workflow
 *     inferred from fps_maintain_wg_staff.js (rowId == null branch triggers POST to api/v1/wgstaff)
 *   - CreatedAtAction response returns Location header pointing to GET /{pactId} for the new resource
 *
 * PHASE 14 SECURITY REVIEW — FIXED 2026-06-11:
 *   - FIXED: Removed spurious leading space from [Authorize] Roles attribute.
 *     Before : "API-FPSUser,API-FPSAdmin, API-FPSShared"  (space before API-FPSShared would have
 *              prevented that role from matching token claim values — silent authz bypass risk)
 *     After  : "API-FPSUser,API-FPSAdmin,API-FPSShared"
 *   PASS:
 *   - [Authorize] class-level covers all 5 CRUD actions; no [AllowAnonymous] overrides.
 *   - [ApiController] auto-returns 400 for null/malformed [FromBody] request.
 *   - No raw SQL in controller — all data access via IWorkGroupEmployeeService (LINQ/EF Core).
 *   - No secrets, connection strings, or credentials in source file.
 *   DEFERRED:
 *   - TRANSFORMENGINE TODO: KeyNotFoundException thrown for 404 cases — confirm global exception
 *     middleware maps to 404, not 500 (avoids leaking exception type in error response body).
 *   - TRANSFORMENGINE TODO: PUT /wgstaff has no route-level ID — PactId comes from request body only.
 *     Verify repository enforces user-context ownership (UserEmailId) to prevent IDOR on update.
 *   - TRANSFORMENGINE TODO: Confirm corrected role values "API-FPSUser,API-FPSAdmin,API-FPSShared"
 *     match exact token claim values in the target deployment identity provider.
 *   - TRANSFORMENGINE TODO: Confirm wgGrade is available in the frontend modal/page state before
 *     frontend phases generate API client calls — wgGrade is a required query param for GET list.
 *
 * PRESERVED:
 *   - [ApiController], [ApiVersion("1.0")], route "api/v{version:apiVersion}/wgstaff" unchanged
 *   - GetWorkGroupEmployeeAsync (GET list with wgGrade filter + pagination) unchanged
 *   - GetWorkGroupEmployeeByIdAsync (GET /{pactId}) unchanged
 *   - UpdateWorkGroupEmployeeAsync (PUT) unchanged — HrsAvail computed server-side (service layer)
 *   - DeleteWorkGroupEmployeeAsync (DELETE /{pactId}) unchanged
 *   - Constructor null guards for IWorkGroupEmployeeService and IMapper unchanged
 *
 * PHASE 6 GATE — VERIFIED 2026-06-11:
 *   Route            : api/v{version:apiVersion}/wgstaff  (versioned, recorded in transform-plan.md)
 *   CRUD actions     : GET /wgstaff (list, wgGrade filter + pagination)
 *                      GET /wgstaff/{pactId} (by id)
 *                      POST /wgstaff (create — CreatedAtAction 201)
 *                      PUT /wgstaff (update)
 *                      DELETE /wgstaff/{pactId} (delete)
 *   Request contract : WorkGroupEmployeeReq (Apha.Common.Contracts.FPS) — confirmed all prototype fields
 *   Response contract: WorkGroupEmployeeRes (Apha.Common.Contracts.FPS) — confirmed all prototype fields
 *   RequestMapper    : CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().ReverseMap() and
 *                      CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().ReverseMap() present
 *                      in RequestMapper.cs (lines 107-108). New fields resolved by name convention.
 *   Lookup endpoints : None required — wgGrade is supplied from parent page context (not a lookup call)
 *   Required params  : wgGrade (required query filter for GET list, sourced from parent page route/state)
 *                      pactId (required path param for GET-by-id and DELETE, sourced from grid row selection)
 *   Frontend binding : POST body = WorkGroupEmployeeReq; GET list returns PaginationRes<WorkGroupEmployeeRes>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm HrsAvail computation for CreateWorkGroupEmployeeAsync is handled
 *     by the service/repository layer (HrsPaid - Leave - SickSpecial), not the caller.
 *   - TRANSFORMENGINE TODO: Confirm WorkGroupEmployeeReq.PactId uniqueness constraint is enforced
 *     by the repository; service should throw InvalidOperationException on duplicate PactId.
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
    /// API controller for WG Staff (employees) within a given WG grade.
    /// </summary>
    // TRANSFORMENGINE: Phase 14 security fix — removed spurious leading space before API-FPSShared.
    // Prior value "API-FPSUser,API-FPSAdmin, API-FPSShared" would not match "API-FPSShared" claim.
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin,API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wgstaff")]
    public class WorkGroupEmployeeController : ControllerBase
    {
        private readonly IWorkGroupEmployeeService _WorkGroupEmployeeService;
        private readonly IMapper _mapper;

        public WorkGroupEmployeeController(IWorkGroupEmployeeService WorkGroupEmployeeService, IMapper mapper)
        {
            _WorkGroupEmployeeService = WorkGroupEmployeeService ?? throw new ArgumentNullException(nameof(WorkGroupEmployeeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of staff for the given WG grade.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="wgGrade">The WG grade code.</param>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroupEmployeeAsync([FromQuery] PaginationReq<string> query, [FromQuery] string wgGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _WorkGroupEmployeeService.GetWorkGroupEmployeeAsync(filter, wgGrade);
            return Ok(_mapper.Map<PaginationRes<WorkGroupEmployeeRes>>(result));
        }

        /// <summary>
        /// Returns a single WG employee by PACTid.
        /// </summary>
        /// <param name="pactId">The PACTid of the employee.</param>
        [HttpGet("{pactId}")]
        public async Task<IActionResult> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            var result = await _WorkGroupEmployeeService.GetWorkGroupEmployeeByIdAsync(pactId);
            if (result == null)
                throw new KeyNotFoundException("WorkGroupEmployee not found.");
            return Ok(_mapper.Map<WorkGroupEmployeeRes>(result));
        }

        // TRANSFORMENGINE: CreateWorkGroupEmployeeAsync added — POST /api/v1/wgstaff
        // Corresponds to the Add/rowId==null branch in fps_maintain_wg_staff.js.
        // Returns 201 Created with Location header pointing to GET /api/v1/wgstaff/{pactId}.
        /// <summary>
        /// Creates a new WG employee record. HrsAvail is computed server-side as HrsPaid - (Leave + SickSpecial).
        /// </summary>
        /// <param name="req">The WG employee create request.</param>
        [HttpPost]
        public async Task<IActionResult> CreateWorkGroupEmployeeAsync([FromBody] WorkGroupEmployeeReq req)
        {
            var dto = _mapper.Map<WorkGroupEmployeeDto>(req);
            var result = await _WorkGroupEmployeeService.CreateWorkGroupEmployeeAsync(dto);
            return CreatedAtAction(
                nameof(GetWorkGroupEmployeeByIdAsync),
                new { pactId = result.PactId },
                _mapper.Map<WorkGroupEmployeeRes>(result));
        }

        /// <summary>
        /// Updates an existing WG employee record. HrsAvail is computed server-side as HrsPaid - (Leave + SickSpecial).
        /// </summary>
        /// <param name="req">The WG employee update request.</param>
        [HttpPut]
        public async Task<IActionResult> UpdateWorkGroupEmployeeAsync([FromBody] WorkGroupEmployeeReq req)
        {
            var dto = _mapper.Map<WorkGroupEmployeeDto>(req);
            var result = await _WorkGroupEmployeeService.UpdateWorkGroupEmployeeAsync(dto);
            return Ok(_mapper.Map<WorkGroupEmployeeRes>(result));
        }

        /// <summary>
        /// Deletes a WG employee by PACTid.
        /// </summary>
        /// <param name="pactId">The PACTid of the employee to delete.</param>
        [HttpDelete("{pactId}")]
        public async Task<IActionResult> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            var isDeleted = await _WorkGroupEmployeeService.DeleteWorkGroupEmployeeAsync(pactId);
            if (!isDeleted)
                throw new KeyNotFoundException("WorkGroupEmployee not found.");
            return Ok(isDeleted);
        }
    }
}
