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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin,API-FPSShared")]
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
        /// Returns a paged list of TestOrProduct VLA entries for the current FPS year.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters.</param>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(_mapper.Map<PaginationRes<TestListVlaRes>>(result));
        }

        /// <summary>
        /// Returns an unpaged list of TestOrProduct VLA entries for the current FPS year.
        /// Used for frontend select-list / lookup population.
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllByYearAsync()
        {
            var result = await _service.GetAllByYearAsync();
            return Ok(_mapper.Map<List<TestListVlaRes>>(result));
        }

        /// <summary>
        /// Returns a single TestOrProduct VLA entry by key (ItemCode) for the current FPS year.
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        [HttpGet("{itemCode}")]
        public async Task<IActionResult> GetByIdAsync(string itemCode)
        {
            var result = await _service.GetByKeyAsync(itemCode);
            if (result == null)
                throw new KeyNotFoundException("Test list entry not found.");
            return Ok(_mapper.Map<TestListVlaRes>(result));
        }

        /// <summary>
        /// Creates a new TestOrProduct VLA entry for the FPS year from the request context.
        /// </summary>
        /// <param name="req">The create request containing all writable fields.</param>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TestListVlaReq req)
        {
            var dto = _mapper.Map<TestListVlaDto>(req);
            var result = await _service.CreateAsync(dto);
            return Ok(_mapper.Map<TestListVlaRes>(result));
        }

        /// <summary>
        /// Updates an existing TestOrProduct VLA entry identified by item code for the current FPS year.
        /// </summary>
        /// <param name="itemCode">The item code (route key).</param>
        /// <param name="req">The update request body.</param>
        [HttpPut("{itemCode}")]
        public async Task<IActionResult> UpdateAsync(string itemCode, [FromBody] TestListVlaReq req)
        {
            var dto = _mapper.Map<TestListVlaDto>(req);
            var result = await _service.UpdateAsync(itemCode, dto);
            return Ok(_mapper.Map<TestListVlaRes>(result));
        }

        /// <summary>
        /// Deletes a TestOrProduct VLA entry by item code for the current FPS year.
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        [HttpDelete("{itemCode}")]
        public async Task<IActionResult> DeleteAsync(string itemCode)
        {
            var isDeleted = await _service.DeleteAsync(itemCode);
            if (!isDeleted)
                throw new KeyNotFoundException("Test list entry not found.");
            return Ok(isDeleted);
        }
    }
}
