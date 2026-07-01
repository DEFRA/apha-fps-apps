/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 5: New [ApiController] REST controller created from fsubTestequirementRCPrice VBA subform CRUD operations
 *   - Phase 5: MS Access DAO subform navigation → ASP.NET Core 10 versioned REST endpoints
 *   - Phase 5: Five endpoints: GetByTestCode (list by test+year), GetByKey (full composite PK), Create, Update, Delete
 *   - Phase 5: Exception-driven flow: throws KeyNotFoundException when record not found
 *   - Phase 5: Composite PK (TestCode + Buyer + ProfitCentre + FpsYear) carried in route for PUT/DELETE
 *   - Phase 6: Readiness gate confirmed — all 5 routes verified against Backend Handoff table
 *   - Phase 6: GET /api/v1/testrequirementrccost/{testCode}/{fpsYear} (list) and /{testCode}/{buyer}/{profitCentre}/{fpsYear} (single) confirmed
 *   - Phase 6: Required parameters: testCode + fpsYear are required business context; buyer from project/requirement selection; profitCentre from component charge row
 *   - Phase 6: Parameters satisfiable — testCode+fpsYear from parent TestListVla selection; buyer from test requirement tab row; profitCentre from RC cost subform row
 *   - Phase 14: Security review PASS — [Authorize] on all actions, no [AllowAnonymous], no raw SQL, no hardcoded secrets,
 *     route/body key consistency (testCode + buyer + profitCentre + fpsYear) enforced in TestRequirementRCCostService.UpdateAsync,
 *     ExceptionMiddleware centralises all exception-to-response mapping without leaking stack traces or connection strings
 *
 * PRESERVED:
 *   - All CRUD operations modelled from fsubTestequirementRCPrice VBA subform (project-specific component charges)
 *   - Composite PK contract: TestCode + Buyer + ProfitCentre + FpsYear
 *   - Subform resource family kept separate from main TestListVla CRUD resource and from TestRCCost resource
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm [Authorize] roles match environment-specific role names
 *   - TRANSFORMENGINE TODO: FK validation (TestCode+Buyer+FpsYear in fps.tlkptestreqmt,
 *     TestCode+ProfitCentre+FpsYear in fps.tbltestrccost) is enforced at service layer
 *   - TRANSFORMENGINE TODO: Add [Required] / data-annotation attributes to TestRequirementRCCostReq (tracked in Phase 3 deferred)
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
    /// API controller for project-specific component charges (TestRequirementRCCost).
    /// Manages CRUD for the fps.tbltestrequirementrccost resource.
    /// Composite PK: TestCode + Buyer + ProfitCentre + FpsYear.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testrequirementrccost")]
    public class TestRequirementRCCostController : ControllerBase
    {
        private readonly ITestRequirementRCCostService _service;
        private readonly IMapper _mapper;

        public TestRequirementRCCostController(ITestRequirementRCCostService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns all project-specific component charges for a given test code and FPS year.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: GET /api/v1/testrequirementrccost/{testCode}/{fpsYear} — list all project charges for a test
        [HttpGet("{testCode}/{fpsYear:int}")]
        public async Task<IActionResult> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            var result = await _service.GetByTestCodeAsync(testCode, fpsYear);
            return Ok(_mapper.Map<List<TestRequirementRCCostRes>>(result));
        }

        /// <summary>
        /// Returns a single project-specific component charge by composite key
        /// (TestCode + Buyer + ProfitCentre + FpsYear).
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="buyer">The buyer (project) code.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: GET /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — single record fetch
        [HttpGet("{testCode}/{buyer}/{profitCentre}/{fpsYear:int}")]
        public async Task<IActionResult> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            var result = await _service.GetByKeyAsync(testCode, buyer, profitCentre, fpsYear);
            if (result == null)
                throw new KeyNotFoundException("Project component charge entry not found.");
            return Ok(_mapper.Map<TestRequirementRCCostRes>(result));
        }

        /// <summary>
        /// Creates a new project-specific component charge entry.
        /// </summary>
        /// <param name="req">The create request containing TestCode, Buyer, ProfitCentre, FpsYear, and Price.</param>
        // TRANSFORMENGINE: POST /api/v1/testrequirementrccost — create new project component charge row
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TestRequirementRCCostReq req)
        {
            var dto = _mapper.Map<TestRequirementRCCostDto>(req);
            var result = await _service.CreateAsync(dto);
            return Ok(_mapper.Map<TestRequirementRCCostRes>(result));
        }

        /// <summary>
        /// Updates an existing project-specific component charge entry identified by composite key.
        /// </summary>
        /// <param name="testCode">The test code (route key).</param>
        /// <param name="buyer">The buyer/project code (route key).</param>
        /// <param name="profitCentre">The profit centre code (route key).</param>
        /// <param name="fpsYear">The FPS year (route key).</param>
        /// <param name="req">The update request body.</param>
        // TRANSFORMENGINE: PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — update project charge
        [HttpPut("{testCode}/{buyer}/{profitCentre}/{fpsYear:int}")]
        public async Task<IActionResult> UpdateAsync(
            string testCode,
            string buyer,
            string profitCentre,
            int fpsYear,
            [FromBody] TestRequirementRCCostReq req)
        {
            var dto = _mapper.Map<TestRequirementRCCostDto>(req);
            var result = await _service.UpdateAsync(testCode, buyer, profitCentre, fpsYear, dto);
            return Ok(_mapper.Map<TestRequirementRCCostRes>(result));
        }

        /// <summary>
        /// Deletes a project-specific component charge entry by composite key.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="buyer">The buyer/project code.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        /// <param name="fpsYear">The FPS year.</param>
        // TRANSFORMENGINE: DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — delete project charge
        [HttpDelete("{testCode}/{buyer}/{profitCentre}/{fpsYear:int}")]
        public async Task<IActionResult> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            var isDeleted = await _service.DeleteAsync(testCode, buyer, profitCentre, fpsYear);
            if (!isDeleted)
                throw new KeyNotFoundException("Project component charge entry not found.");
            return Ok(isDeleted);
        }
    }
}
