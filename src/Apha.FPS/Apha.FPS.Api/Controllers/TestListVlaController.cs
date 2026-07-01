/*
 * TRANSFORMENGINE MIGRATION — TestListVlaController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 5: New [ApiController] REST controller created from frmTestList / fsubTest_MainList VBA CRUD operations
 *   - Phase 5: MS Access DAO/form navigation → ASP.NET Core 10 versioned REST endpoints
 *   - Phase 5: Six endpoints: GetAll (paged, fpsYear filter), GetAllByYear (lookup), GetById (composite key), Create, Update, Delete
 *   - Phase 5: Exception-driven flow: throws KeyNotFoundException when record not found
 *   - Phase 5: Composite PK (ItemCode + FpsYear) carried in both route and body for PUT/DELETE
 *   - Phase 5: All actions require [Authorize] with API-FPSUser or API-FPSAdmin roles
 *   - Phase 6: Readiness gate confirmed — all 6 routes verified against Backend Handoff table
 *   - Phase 6: Lookup endpoint GET /api/v1/testlistvla/lookup?fpsYear={year} recorded in Backend Handoff table
 *   - Phase 6: Required parameters confirmed — fpsYear is required query param for GetAll/lookup; itemCode+fpsYear are route keys for GetById/Update/Delete
 *   - Phase 6: All required business parameters (fpsYear partition filter) are satisfiable from the page's year-selector control
 *   - Phase 14: Security review PASS — [Authorize] on all actions, no [AllowAnonymous], no raw SQL, no hardcoded secrets,
 *     route/body key consistency enforced in TestListVlaService.UpdateAsync, ExceptionMiddleware centralises all
 *     exception-to-response mapping (no stack traces or connection strings in responses)
 *
 * PRESERVED:
 *   - All CRUD operations modelled from frmTestList / fsubTest_MainList VBA form
 *   - Composite PK contract: ItemCode + FpsYear
 *   - Pricing fields: UnitPriceVla, PriceAhvg, DefraUnitPrice
 *   - Lookup endpoint separation: /lookup kept separate from main CRUD resource family
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm [Authorize] roles match environment-specific role names
 *   - TRANSFORMENGINE TODO: owner field (PT/PA/SD/LT) validation enforced at service layer
 *   - TRANSFORMENGINE TODO: Add [Required] / data-annotation attributes to TestListVlaReq (tracked in Phase 3 deferred)
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
    /// API controller for TestOrProduct VLA list management.
    /// Manages test list CRUD for the VLA (fps.testorproduct) resource.
    /// Composite PK: ItemCode + FpsYear.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testlistvla")]
    public class TestListVlaController : ControllerBase
    {
        private readonly ITestListVlaService _service;
        private readonly IMapper _mapper;

        public TestListVlaController(ITestListVlaService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paged list of TestOrProduct VLA entries for a given FPS year.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters.</param>
        /// <param name="fpsYear">The FPS year to filter by (required).</param>
        // TRANSFORMENGINE: GET /api/v1/testlistvla?fpsYear={year} — paged list, maps to GetAllAsync
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] int fpsYear)
        {
            var result = await _service.GetAllAsync(query, fpsYear);
            return Ok(_mapper.Map<PaginationRes<TestListVlaRes>>(result));
        }

        /// <summary>
        /// Returns an unpaged list of TestOrProduct VLA entries for a given FPS year.
        /// Used for frontend select-list / lookup population.
        /// </summary>
        /// <param name="fpsYear">The FPS year to filter by (required).</param>
        // TRANSFORMENGINE: GET /api/v1/testlistvla/lookup?fpsYear={year} — unpaged lookup, maps to GetAllByYearAsync
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllByYearAsync([FromQuery] int fpsYear)
        {
            var result = await _service.GetAllByYearAsync(fpsYear);
            return Ok(_mapper.Map<List<TestListVlaRes>>(result));
        }

        /// <summary>
        /// Returns a single TestOrProduct VLA entry by composite key (ItemCode + FpsYear).
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: GET /api/v1/testlistvla/{itemCode}/{fpsYear} — single record fetch
        [HttpGet("{itemCode}/{fpsYear:int}")]
        public async Task<IActionResult> GetByIdAsync(string itemCode, int fpsYear)
        {
            var result = await _service.GetByKeyAsync(itemCode, fpsYear);
            if (result == null)
                throw new KeyNotFoundException("Test list entry not found.");
            return Ok(_mapper.Map<TestListVlaRes>(result));
        }

        /// <summary>
        /// Creates a new TestOrProduct VLA entry.
        /// </summary>
        /// <param name="req">The create request containing all writable fields.</param>
        // TRANSFORMENGINE: POST /api/v1/testlistvla — create new VLA test record
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TestListVlaReq req)
        {
            var dto = _mapper.Map<TestListVlaDto>(req);
            var result = await _service.CreateAsync(dto);
            return Ok(_mapper.Map<TestListVlaRes>(result));
        }

        /// <summary>
        /// Updates an existing TestOrProduct VLA entry identified by composite key.
        /// </summary>
        /// <param name="itemCode">The item code (route key).</param>
        /// <param name="fpsYear">The FPS year (route key).</param>
        /// <param name="req">The update request body.</param>
        // TRANSFORMENGINE: PUT /api/v1/testlistvla/{itemCode}/{fpsYear} — update VLA test record
        [HttpPut("{itemCode}/{fpsYear:int}")]
        public async Task<IActionResult> UpdateAsync(string itemCode, int fpsYear, [FromBody] TestListVlaReq req)
        {
            var dto = _mapper.Map<TestListVlaDto>(req);
            var result = await _service.UpdateAsync(itemCode, fpsYear, dto);
            return Ok(_mapper.Map<TestListVlaRes>(result));
        }

        /// <summary>
        /// Deletes a TestOrProduct VLA entry by composite key.
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: DELETE /api/v1/testlistvla/{itemCode}/{fpsYear} — delete VLA test record
        [HttpDelete("{itemCode}/{fpsYear:int}")]
        public async Task<IActionResult> DeleteAsync(string itemCode, int fpsYear)
        {
            var isDeleted = await _service.DeleteAsync(itemCode, fpsYear);
            if (!isDeleted)
                throw new KeyNotFoundException("Test list entry not found.");
            return Ok(isDeleted);
        }
    }
}
