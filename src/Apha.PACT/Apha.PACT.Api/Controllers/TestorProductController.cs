using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for Test List operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testorproduct")]
    public class TestorProductController : ControllerBase
    {
        private readonly ITestorProductService _service;
        private readonly IMapper _mapper;

        public TestorProductController(ITestorProductService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<TestorProductRes>>> GetAllTestorProductsAsync()
        {
            var items = await _service.GetAllTestorProductsAsync();
            return Ok(items.Select(i => new TestorProductRes
            {
                ItemCode = i.ItemCode,
                ItemDescription = i.ItemDescription
            }).ToList());
        }

        /// <summary>Retrieves a paginated list of Test or Product records.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query)
        {
            var pagedResult = await _service.GetPagedTestOrProductsAsync(query);
            var mappedResult = _mapper.Map<PaginationRes<TestorProductRes>>(pagedResult);
            // Wrap in ApiResponse to match expected format by Web application
            var response = new ApiResponse<PaginationRes<TestorProductRes>>
            {
                Success = true,
                Data = mappedResult,
            };

            return Ok(response);
        }

        /// <summary>Retrieves a Test or Product record by ItemCode.</summary>
        [HttpGet("{itemCode}")]
        public async Task<IActionResult> GetById(string itemCode)
        {
            var item = await _service.GetTestorProductByIdAsync(itemCode);
            if (item == null)
            {
                throw new KeyNotFoundException($"Test or Product with ItemCode {itemCode} not found.");
            }
            return Ok(_mapper.Map<TestorProductRes>(item));
        }

        /// <summary>Creates a new Test or Product record.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TestorProductReq request)
        {
            var dto = _mapper.Map<TestorProductDto>(request);
            var created = await _service.CreateTestorProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { itemCode = created.ItemCode }, _mapper.Map<TestorProductRes>(created));
        }

        /// <summary>Updates an existing Test or Product record.</summary>
        [HttpPut("{itemCode}")]
        public async Task<IActionResult> Update(string itemCode, [FromBody] TestorProductReq request)
        {
            var dto = _mapper.Map<TestorProductDto>(request);
            dto.ItemCode = itemCode;
            var updated = await _service.UpdateTestorProductAsync(dto);
            return Ok(_mapper.Map<TestorProductRes>(updated));
        }

        /// <summary>Deletes a Test or Product record.</summary>
        [HttpDelete("{itemCode}")]
        public async Task<IActionResult> Delete(string itemCode)
        {
            var deleted = await _service.DeleteTestorProductAsync(itemCode);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Test or Product with ItemCode {itemCode} not found for deletion.");
            }
            return Ok(deleted);
        }

        /// <summary>Retrieves distinct owner values.</summary>
        [HttpGet("owners")]
        public async Task<IActionResult> GetOwners()
        {
            var owners = await _service.GetOwnersAsync();
            return Ok(owners);
        }
    }
}
