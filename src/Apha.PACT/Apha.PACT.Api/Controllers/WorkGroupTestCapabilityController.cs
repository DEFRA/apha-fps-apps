using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for WorkGroupTestCapability operations.
    /// </summary>
    //[Authorize(Roles = "API-FPSUser,API-FPSAdmin")]    
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgrouptestcapability")]
    public class WorkGroupTestCapabilityController : ControllerBase
    {
        private readonly IWorkGroupTestCapabilityService _service;
        private readonly IMapper _mapper;

        public WorkGroupTestCapabilityController(IWorkGroupTestCapabilityService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // ── TEST CAPABILITY (Grid 1) ──────────────────────────────────────────

        /// <summary>Retrieves a paged list of TestCapability records filtered by WorkGroup.</summary>
        [HttpGet("paged/workgroup")]
        public async Task<IActionResult> GetPagedByWorkGroup(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup)
        {
            var result = await _service.GetPagedByWorkGroupAsync(query, workGroup);
            return Ok(_mapper.Map<PaginationRes<TestCapabilityRes>>(result));
        }

        /// <summary>Retrieves a paged list of TestCapability records filtered by TestCode.</summary>
        [HttpGet("paged/testcode")]
        public async Task<IActionResult> GetPagedByTestCode(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? testCode)
        {
            var result = await _service.GetPagedByTestCodeAsync(query, testCode);
            return Ok(_mapper.Map<PaginationRes<TestCapabilityRes>>(result));
        }

        /// <summary>Retrieves a TestCapability record by composite key.</summary>
        [HttpGet("testcapability/{testCode}/{workGroup}")]
        public async Task<IActionResult> GetTestCapabilityById(string testCode, string workGroup)
        {
            var result = await _service.GetTestCapabilityByIdAsync(testCode, workGroup);
            if (result is null)
                throw new KeyNotFoundException($"TestCapability with TestCode '{testCode}' and WorkGroup '{workGroup}' not found.");
            return Ok(_mapper.Map<TestCapabilityRes>(result));
        }

        /// <summary>Creates a new TestCapability record.</summary>
        [HttpPost("testcapability")]
        public async Task<IActionResult> CreateTestCapability([FromBody] TestCapabilityReq request)
        {
            var dto = _mapper.Map<TestCapabilityDto>(request);
            var result = await _service.AddTestCapabilityAsync(dto);
            return Ok(_mapper.Map<TestCapabilityRes>(result));
        }

        /// <summary>Updates an existing TestCapability record.</summary>
        [HttpPut("testcapability")]
        public async Task<IActionResult> UpdateTestCapability([FromBody] TestCapabilityReq request)
        {
            var dto = _mapper.Map<TestCapabilityDto>(request);
            var result = await _service.UpdateTestCapabilityAsync(dto);
            return Ok(_mapper.Map<TestCapabilityRes>(result));
        }

        /// <summary>Deletes a TestCapability record by composite key.</summary>
        [HttpDelete("testcapability/{testCode}/{workGroup}")]
        public async Task<IActionResult> DeleteTestCapability(string testCode, string workGroup)
        {
            var deleted = await _service.DeleteTestCapabilityAsync(testCode, workGroup);
            return Ok(deleted);
        }

        // ── LOOKUPS ───────────────────────────────────────────────────────────

        /// <summary>Retrieves all TestorProduct items for dropdown population.</summary>
        [HttpGet("testorproducts")]
        public async Task<IActionResult> GetAllTestorProducts()
        {
            var items = await _service.GetAllTestorProductsAsync();
            return Ok(_mapper.Map<IEnumerable<TestorProductRes>>(items));
        }
    }
}
