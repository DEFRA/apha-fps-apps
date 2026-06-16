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
    /// API controller for managing account categories.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accountcategory")]
    public class AccountCategoryController : ControllerBase
    {
        private readonly IAccountCategoryService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountCategoryController"/> class.
        /// </summary>
        /// <param name="service">Service for account category operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public AccountCategoryController(IAccountCategoryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paged list of account categories.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="filterType">Filter type: 'rc' for Resource Centres, 'ps' for Project Specific, or 'all' for all categories.</param>
        /// <returns>A paged list of account category records.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationReq<string> query, [FromQuery] string? filterType = null)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetAllAsync(filter, filterType);
            return Ok(_mapper.Map<PaginationRes<AccountCategoryRes>>(result));
        }

        /// <summary>
        /// Retrieves a single account category by AccShortName.
        /// </summary>
        /// <param name="accShortName">The account short name.</param>
        /// <returns>The account category record.</returns>
        [HttpGet("{accShortName}")]
        public async Task<IActionResult> GetByIdAsync(string accShortName)
        {
            var result = await _service.GetByIdAsync(accShortName);
            if (result == null)
                throw new KeyNotFoundException("Account category not found.");
            return Ok(_mapper.Map<AccountCategoryRes>(result));
        }

        /// <summary>
        /// Adds a new account category record.
        /// </summary>
        /// <param name="req">The account category request data.</param>
        /// <returns>The created account category record.</returns>
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AccountCategoryReq req)
        {
            var dto = _mapper.Map<AccountCategoryDto>(req);
            var result = await _service.AddAsync(dto);
            return Ok(_mapper.Map<AccountCategoryRes>(result));
        }

        /// <summary>
        /// Updates an existing account category record.
        /// </summary>
        /// <param name="accShortName">The original account short name to identify the record.</param>
        /// <param name="req">The account category request data with updated values.</param>
        /// <returns>The updated account category record.</returns>
        [HttpPut("{accShortName}")]
        public async Task<IActionResult> UpdateAsync(string accShortName, [FromBody] AccountCategoryReq req)
        {
            var dto = _mapper.Map<AccountCategoryDto>(req);
            var result = await _service.UpdateAsync(accShortName, dto);
            return Ok(_mapper.Map<AccountCategoryRes>(result));
        }

        /// <summary>
        /// Deletes an account category record by AccShortName.
        /// </summary>
        /// <param name="accShortName">The account short name.</param>
        /// <returns>No content if deletion is successful; NotFound if not found.</returns>
        [HttpDelete("{accShortName}")]
        public async Task<IActionResult> DeleteAsync(string accShortName)
        {
            var isDeleted = await _service.DeleteAsync(accShortName);
            if (!isDeleted)
                throw new KeyNotFoundException("Account category not found.");
            return Ok(isDeleted);
        }
    }
}
