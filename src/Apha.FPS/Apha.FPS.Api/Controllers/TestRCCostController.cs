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
