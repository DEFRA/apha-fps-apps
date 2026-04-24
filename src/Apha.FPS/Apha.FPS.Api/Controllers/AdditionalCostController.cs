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
    /// API controller for managing additional cost plan entries.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/additionalcost")]
    public class AdditionalCostController : ControllerBase
    {
        private readonly IAdditionalCostService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalCostController"/> class.
        /// </summary>
        /// <param name="service">Service for additional cost operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public AdditionalCostController(IAdditionalCostService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paged list of additional costs for a given job code.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="jobCode">The job code to filter by.</param>
        /// <returns>A paged list of additional cost records.</returns>
        [HttpGet]
        public async Task<IActionResult> GetByJobCodeAsync([FromQuery] PaginationReq<string> query, [FromQuery] string jobCode)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetByJobCodeAsync(filter, jobCode);
            return Ok(_mapper.Map<PaginationRes<AdditionalCostRes>>(result));
        }

        /// <summary>
        /// Returns the total item cost for a given job code.
        /// </summary>
        /// <param name="jobCode">The job code.</param>
        /// <returns>Total item cost as a decimal.</returns>
        [HttpGet("totalitemcost")]
        public async Task<IActionResult> GetTotalItemCostAsync([FromQuery] string jobCode)
        {
            var total = await _service.GetTotalItemCostAsync(jobCode);
            return Ok(total);
        }

        /// <summary>
        /// Returns account categories available for additional cost entries.
        /// </summary>
        /// <returns>List of account categories.</returns>
        [HttpGet("accountcategories")]
        public async Task<IActionResult> GetAccountCategoriesAsync()
        {
            var categories = await _service.GetAccountCategoriesAsync();
            return Ok(_mapper.Map<List<AccountCategoryRes>>(categories));
        }

        /// <summary>
        /// Retrieves a single additional cost record by composite key.
        /// </summary>
        /// <param name="jobCode">The job code.</param>
        /// <param name="account">The account short name.</param>
        /// <param name="description">The description.</param>
        /// <returns>The additional cost record.</returns>
        [HttpGet("{jobCode}/{account}/{description}")]
        public async Task<IActionResult> GetByIdAsync(string jobCode, string account, string description)
        {
            var result = await _service.GetByIdAsync(jobCode, account, description);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<AdditionalCostRes>(result));
        }

        /// <summary>
        /// Adds a new additional cost record.
        /// </summary>
        /// <param name="req">The additional cost request data.</param>
        /// <returns>The created additional cost record.</returns>
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AdditionalCostReq req)
        {
            var dto = _mapper.Map<AdditionalCostDto>(req);
            var result = await _service.AddAsync(dto);
            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { jobCode = result.JobCode, account = result.Account, description = result.Description },
                _mapper.Map<AdditionalCostRes>(result));
        }

        /// <summary>
        /// Updates an existing additional cost record.
        /// </summary>
        /// <param name="req">The additional cost request data.</param>
        /// <returns>The updated additional cost record.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] AdditionalCostReq req)
        {
            var dto = _mapper.Map<AdditionalCostDto>(req);
            var result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<AdditionalCostRes>(result));
        }

        /// <summary>
        /// Deletes an additional cost record by composite key.
        /// </summary>
        /// <param name="jobCode">The job code.</param>
        /// <param name="account">The account short name.</param>
        /// <param name="description">The description.</param>
        /// <returns>No content if deletion is successful; NotFound if not found.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string jobCode, [FromQuery] string account, [FromQuery] string description)
        {
            var isDeleted = await _service.DeleteAsync(jobCode, account, description);
            if (!isDeleted)
                throw new KeyNotFoundException("Data not found.");
            return Ok(isDeleted);
        }
    }
}
