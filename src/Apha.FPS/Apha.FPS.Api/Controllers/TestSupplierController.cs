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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testsupplier")]
    public class TestSupplierController : ControllerBase
    {
        private readonly ITestSupplierService _service;
        private readonly IMapper _mapper;

        public TestSupplierController(ITestSupplierService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string testCode,
            [FromQuery] bool showRejected = false)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPagedByTestCodeAsync(filter, testCode, showRejected);
            return Ok(_mapper.Map<PaginationRes<TestSupplierViewRes>>(result));
        }

        [HttpGet("{testCode}/{buyer}")]
        public async Task<IActionResult> GetByIdAsync(string testCode, string buyer)
        {
            var result = await _service.GetByIdAsync(testCode, buyer);
            if (result == null)
                return NotFound();
            return Ok(_mapper.Map<TestRequirementRes>(result));
        }

        [HttpGet("testorproducts")]
        public async Task<IActionResult> GetTestOrProductsAsync()
        {
            var result = await _service.GetTestOrProductsAsync();
            return Ok(_mapper.Map<List<TestorProductRes>>(result));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TestRequirementReq req)
        {
            var dto = _mapper.Map<TestRequirementDto>(req);
            var result = await _service.AddAsync(dto);
            return Ok(_mapper.Map<TestRequirementRes>(result));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TestRequirementReq req)
        {
            var dto = _mapper.Map<TestRequirementDto>(req);
            var result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<TestRequirementRes>(result));
        }

        [HttpDelete("{testCode}/{buyer}")]
        public async Task<IActionResult> DeleteAsync(string testCode, string buyer)
        {
            try
            {
                var success = await _service.DeleteAsync(testCode, buyer);
                if (!success)
                    return NotFound();
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
