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

        // ── TEST REQMT (Grid 2) ───────────────────────────────────────────────

        /// <summary>Retrieves a paged list of TestReqmt records for a given TestCode.</summary>
        [HttpGet("testreqmt/paged/{testCode}")]
        public async Task<IActionResult> GetPagedTestReqmt(
            [FromQuery] QueryParameters<string> query,
            string testCode)
        {
            var result = await _service.GetPagedTestReqmtAsync(query, testCode);
            return Ok(_mapper.Map<PaginationRes<TestReqmtRes>>(result));
        }

        /// <summary>Retrieves all TestReqmt records for a given TestCode without pagination (for export).</summary>
        [HttpGet("testreqmt/all/{testCode}")]
        public async Task<IActionResult> GetAllTestReqmtForExport(string testCode, [FromQuery] string? filter = null)
        {
            var items = await _service.GetAllTestReqmtForExportAsync(testCode, filter);
            return Ok(_mapper.Map<IEnumerable<TestReqmtRes>>(items));
        }

        /// <summary>Retrieves a TestReqmt record by composite key.</summary>
        [HttpGet("testreqmt/{testCode}/{buyer}")]
        public async Task<IActionResult> GetTestReqmtById(string testCode, string buyer)
        {
            var result = await _service.GetTestReqmtByIdAsync(testCode, buyer);
            if (result is null)
                throw new KeyNotFoundException($"TestReqmt with TestCode '{testCode}' and Buyer '{buyer}' not found.");
            return Ok(_mapper.Map<TestReqmtRes>(result));
        }

        /// <summary>Creates a new TestReqmt record.</summary>
        [HttpPost("testreqmt")]
        public async Task<IActionResult> CreateTestReqmt([FromBody] TestReqmtReq request)
        {
            var dto = _mapper.Map<TestReqmtDto>(request);
            var result = await _service.AddTestReqmtAsync(dto);
            return Ok(_mapper.Map<TestReqmtRes>(result));
        }

        /// <summary>Updates an existing TestReqmt record.</summary>
        [HttpPut("testreqmt")]
        public async Task<IActionResult> UpdateTestReqmt([FromBody] TestReqmtReq request)
        {
            var dto = _mapper.Map<TestReqmtDto>(request);
            var result = await _service.UpdateTestReqmtAsync(dto);
            return Ok(_mapper.Map<TestReqmtRes>(result));
        }

        /// <summary>Deletes a TestReqmt record by composite key.</summary>
        [HttpDelete("testreqmt/{testCode}/{buyer}")]
        public async Task<IActionResult> DeleteTestReqmt(string testCode, string buyer)
        {
            var deleted = await _service.DeleteTestReqmtAsync(testCode, buyer);
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

        /// <summary>Looks up RecUnitPrice and IsDefraProject. ProjectCode is optional — omitting it returns DefraUnitPrice by default.</summary>
        [HttpGet("testreqmt/pricing")]
        public async Task<IActionResult> GetTestReqmtPricing(
            [FromQuery] string testCode, [FromQuery] string? projectCode = null)
        {
            var result = await _service.GetTestReqmtPricingAsync(testCode, projectCode);
            if (result is null)
                return NotFound($"No pricing found for TestCode '{testCode}'.");
            return Ok(_mapper.Map<TestReqmtRes>(result));
        }
    }
}
