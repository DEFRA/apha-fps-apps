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
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroup")]
    public class WorkGroupController : ControllerBase
    {
        private readonly IWorkGroupService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="WorkGroupController"/> with the required
        /// work group service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve work group and time code data.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public WorkGroupController(IWorkGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all work groups available in the system.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{WorkGroupRes}"/> containing all work groups.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllWorkGroupsAsync();
            return Ok(_mapper.Map<IEnumerable<WorkGroupRes>>(items));
        }

        /// <summary>
        /// Retrieves a paged and sorted list of time codes associated with work groups,
        /// optionally filtered by work group name and calendar month.
        /// </summary>
        /// <param name="query">Pagination, sorting, and column filter parameters for the request.</param>
        /// <param name="workGroup">Optional work group name to restrict results to a specific work group.</param>
        /// <param name="monthNumber">Optional calendar month number to restrict results to a specific month.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{WorkGroupTimeCodeRes}"/> containing the paged time code records
        /// and associated pagination metadata.
        /// </returns>
        [HttpGet("paged/timecodes")]
        public async Task<IActionResult> GetPagedWorkGroupTimeCodes(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workGroup,
            [FromQuery] int monthNumber = 1)
        {
            var result = await _service.GetWorkGroupTimeCodeAsync(query, workGroup, monthNumber);
            return Ok(_mapper.Map<PaginationRes<WorkGroupTimeCodeRes>>(result));
        }

        /// <summary>
        /// Retrieves a paged and sorted list of valid time codes associated with work groups,
        /// optionally filtered by work group name. Each record joins TimeCodeValid with the
        /// corresponding Project to include the project title.
        /// </summary>
        /// <param name="query">Pagination, sorting, and column filter parameters for the request.</param>
        /// <param name="workGroup">Optional work group name to restrict results to a specific work group.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{WorkGroupValidTimeCodeRes}"/> containing the paged valid time code records
        /// and associated pagination metadata.
        /// </returns>
        [HttpGet("paged/validtimecodes")]
        public async Task<IActionResult> GetPagedWorkGroupValidTimeCodes(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workGroup)
        {
            var result = await _service.GetWorkGroupValidTimeCodeAsync(query, workGroup);
            return Ok(_mapper.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(result));
        }
    }
}
