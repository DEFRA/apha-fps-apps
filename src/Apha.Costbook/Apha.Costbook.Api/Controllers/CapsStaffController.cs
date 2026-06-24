/*
 * TRANSFORMENGINE MIGRATION — CapsStaffController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New [ApiController] created from MS Access frmMaintainance Tab 5 (CAPS Staff)
 *   - Full CRUD: GET all, GET by mNumber, POST, PUT, DELETE
 *   - Routes: GET  /api/v1/capsstaff
 *             GET  /api/v1/capsstaff/{mNumber}
 *             POST /api/v1/capsstaff
 *             PUT  /api/v1/capsstaff/{mNumber}
 *             DELETE /api/v1/capsstaff/{mNumber}
 *   - Depends on ICapsStaffService (Phase 3)
 *   - Uses AutoMapper for CapsStaffDto <-> CapsStaffReq/Res conversions
 *   - POST returns CreatedAtAction pointing to GetCapsStaff (resource identity = MNumber)
 *   - Authorization: [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")] at controller level;
 *     mutating actions (POST/PUT/DELETE) additionally restricted to API-CostbookAdmin
 *   - Phase 14 security: [Required] and [MaxLength(50)] added to CapsStaffReq.MNumber and [MaxLength(50)]
 *     to CapsStaffReq.Name in Apha.Common — enforces DB varchar(50) constraints at model binding layer
 *
 * PRESERVED:
 *   - All service operation semantics preserved from ICapsStaffService
 *   - Exception-driven flow (ArgumentException, KeyNotFoundException) handled by ExceptionMiddleware
 *   - MNumber PK from CapsStaffReq.MNumber on create; route param on update/delete
 *   - string.IsNullOrWhiteSpace(mNumber) guard on DELETE preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number should be writable via PUT (not in HTML prototype modal)
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
    // TRANSFORMENGINE: Full CRUD controller for mabarchive.tblcapsstaff — covers Tab 5 (CAPS Staff) of frmMaintainance
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/capsstaff")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class CapsStaffController : ControllerBase
    {
        private readonly ICapsStaffService _service;
        private readonly IMapper _mapper;

        public CapsStaffController(ICapsStaffService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all CAPS staff members ordered by MNumber.
        /// Drives the Tab 5 data grid in frmMaintainance.
        /// </summary>
        /// <returns>200 OK with list of <see cref="CapsStaffRes"/> entries.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCapsStaff()
        {
            // TRANSFORMENGINE: GET /api/v1/capsstaff — returns full list for Tab 5 grid
            var dtos = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<CapsStaffRes>>(dtos));
        }

        /// <summary>
        /// Returns a paginated list of CAPS staff members.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <returns>200 OK with paginated <see cref="CapsStaffRes"/> results.</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedCapsStaff([FromQuery] PaginationReq<string> query)
        {
            // TRANSFORMENGINE: GET /api/v1/capsstaff/paginated — paginated Tab 5 grid
            var parameters = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPaginatedAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<CapsStaffRes>>(result));
        }

        /// <summary>
        /// Returns a single CAPS staff member by MNumber.
        /// </summary>
        /// <param name="mNumber">Staff member M-Number (primary key).</param>
        /// <returns>200 OK with <see cref="CapsStaffRes"/>, or 404 if not found.</returns>
        [HttpGet("{mNumber}")]
        public async Task<IActionResult> GetCapsStaff(string mNumber)
        {
            // TRANSFORMENGINE: GET /api/v1/capsstaff/{mNumber} — single record lookup
            var dto = await _service.GetByMNumberAsync(mNumber);
            if (dto == null) return NotFound();
            return Ok(_mapper.Map<CapsStaffRes>(dto));
        }

        /// <summary>
        /// Creates a new CAPS staff member.
        /// </summary>
        /// <param name="req">CAPS staff fields. MNumber must be unique.</param>
        /// <returns>201 Created with <see cref="CapsStaffRes"/> of the new record.</returns>
        [HttpPost]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> AddCapsStaff([FromBody] CapsStaffReq req)
        {
            // TRANSFORMENGINE: POST /api/v1/capsstaff — create from Tab 5 modal (formTblCapsStaff)
            var dto = _mapper.Map<CapsStaffDto>(req);
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetCapsStaff), new { mNumber = created.MNumber }, _mapper.Map<CapsStaffRes>(created));
        }

        /// <summary>
        /// Updates an existing CAPS staff member.
        /// </summary>
        /// <param name="mNumber">Staff member M-Number (route key).</param>
        /// <param name="req">Updated CAPS staff fields.</param>
        /// <returns>200 OK with updated <see cref="CapsStaffRes"/>.</returns>
        [HttpPut("{mNumber}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateCapsStaff(string mNumber, [FromBody] CapsStaffReq req)
        {
            // TRANSFORMENGINE: PUT /api/v1/capsstaff/{mNumber} — update from Tab 5 edit modal
            var dto = _mapper.Map<CapsStaffDto>(req);
            var updated = await _service.UpdateAsync(mNumber, dto);
            return Ok(_mapper.Map<CapsStaffRes>(updated));
        }

        /// <summary>
        /// Deletes the CAPS staff member identified by MNumber.
        /// </summary>
        /// <param name="mNumber">Staff member M-Number (route key).</param>
        /// <returns>204 No Content on success.</returns>
        [HttpDelete("{mNumber}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> DeleteCapsStaff(string mNumber)
        {
            // TRANSFORMENGINE: DELETE /api/v1/capsstaff/{mNumber} — delete from Tab 5 confirm modal
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber is required for deletion.");

            await _service.DeleteAsync(mNumber);
            return NoContent();
        }
    }
}
