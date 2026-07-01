/*
 * TRANSFORMENGINE MIGRATION — TestRCCostController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 5: New [ApiController] REST controller created from fsubTestRCPrice VBA subform CRUD operations
 *   - Phase 5: MS Access DAO subform navigation → ASP.NET Core 10 versioned REST endpoints
 *   - Phase 5: Five endpoints: GetByTestCode (list by test+year), GetByKey (full composite PK), Create, Update, Delete
 *   - Phase 5: Exception-driven flow: throws KeyNotFoundException when record not found
 *   - Phase 5: Composite PK (TestCode + ProfitCentre + FpsYear) carried in route for PUT/DELETE
 *   - Phase 6: Readiness gate confirmed — all 5 routes verified against Backend Handoff table
 *   - Phase 6: GET /api/v1/testrccost/{testCode}/{fpsYear} (list) and /{testCode}/{profitCentre}/{fpsYear} (single) both confirmed
 *   - Phase 6: Required parameters: testCode + fpsYear are required business context from the parent test row; profitCentre from dropdown
 *   - Phase 6: Parameters are satisfiable: testCode+fpsYear supplied from parent TestListVla row selection; profitCentre from lookup dropdown
 *   - Phase 14: Security review PASS — [Authorize] on all actions, no [AllowAnonymous], no raw SQL, no hardcoded secrets,
 *     route/body key consistency (testCode + profitCentre + fpsYear) enforced in TestRCCostService.UpdateAsync,
 *     ExceptionMiddleware centralises all exception-to-response mapping without leaking internals
 *
 * PRESERVED:
 *   - All CRUD operations modelled from fsubTestRCPrice VBA subform (general component charges)
 *   - Composite PK contract: TestCode + ProfitCentre + FpsYear
 *   - Subform resource family kept separate from main TestListVla CRUD resource
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm [Authorize] roles match environment-specific role names
 *   - TRANSFORMENGINE TODO: FK validation (TestCode+FpsYear in fps.testorproduct,
 *     ProfitCentre in fps.tblkpprofitcentre) is enforced at service layer
 *   - TRANSFORMENGINE TODO: Add [Required] / data-annotation attributes to TestRCCostReq (tracked in Phase 3 deferred)
 */

using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for component charges per profit centre (TestRCCost).
    /// Manages CRUD for the fps.tbltestrccost resource.
    /// Composite PK: TestCode + ProfitCentre + FpsYear.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testrccost")]
    public class TestRCCostController : ControllerBase
    {
        private readonly ITestRCCostService _service;
        private readonly IMapper _mapper;

        public TestRCCostController(ITestRCCostService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns all component charges for a given test code and FPS year.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: GET /api/v1/testrccost/{testCode}/{fpsYear} — list all profit-centre charges for a test
        [HttpGet("{testCode}/{fpsYear:int}")]
        public async Task<IActionResult> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            var result = await _service.GetByTestCodeAsync(testCode, fpsYear);
            return Ok(_mapper.Map<List<TestRCCostRes>>(result));
        }

        /// <summary>
        /// Returns a single component charge by composite key (TestCode + ProfitCentre + FpsYear).
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: GET /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — single record fetch by full PK
        [HttpGet("{testCode}/{profitCentre}/{fpsYear:int}")]
        public async Task<IActionResult> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            var result = await _service.GetByKeyAsync(testCode, profitCentre, fpsYear);
            if (result == null)
                throw new KeyNotFoundException("Component charge entry not found.");
            return Ok(_mapper.Map<TestRCCostRes>(result));
        }

        /// <summary>
        /// Creates a new component charge entry.
        /// </summary>
        /// <param name="req">The create request containing TestCode, ProfitCentre, FpsYear, and Price.</param>
        // TRANSFORMENGINE: POST /api/v1/testrccost — create new component charge row
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TestRCCostReq req)
        {
            var dto = _mapper.Map<TestRCCostDto>(req);
            var result = await _service.CreateAsync(dto);
            return Ok(_mapper.Map<TestRCCostRes>(result));
        }

        /// <summary>
        /// Updates an existing component charge entry identified by composite key.
        /// </summary>
        /// <param name="testCode">The test code (route key).</param>
        /// <param name="profitCentre">The profit centre code (route key).</param>
        /// <param name="fpsYear">The FPS year (route key).</param>
        /// <param name="req">The update request body.</param>
        // TRANSFORMENGINE: PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — update component charge
        [HttpPut("{testCode}/{profitCentre}/{fpsYear:int}")]
        public async Task<IActionResult> UpdateAsync(string testCode, string profitCentre, int fpsYear, [FromBody] TestRCCostReq req)
        {
            var dto = _mapper.Map<TestRCCostDto>(req);
            var result = await _service.UpdateAsync(testCode, profitCentre, fpsYear, dto);
            return Ok(_mapper.Map<TestRCCostRes>(result));
        }

        /// <summary>
        /// Deletes a component charge entry by composite key.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — delete component charge
        [HttpDelete("{testCode}/{profitCentre}/{fpsYear:int}")]
        public async Task<IActionResult> DeleteAsync(string testCode, string profitCentre, int fpsYear)
        {
            var isDeleted = await _service.DeleteAsync(testCode, profitCentre, fpsYear);
            if (!isDeleted)
                throw new KeyNotFoundException("Component charge entry not found.");
            return Ok(isDeleted);
        }
    }
}
