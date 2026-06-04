using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for Monthly TIME Log of Imports (MT_LOG) operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/monthlytime")]
    public class MonthlyTimeController : ControllerBase
    {
        private readonly IMonthlyTimeService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="MonthlyTimeController"/>.
        /// </summary>
        /// <param name="service">Application service for MT_LOG search operations.</param>
        /// <param name="mapper">AutoMapper instance.</param>
        public MonthlyTimeController(IMonthlyTimeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Searches the Monthly TIME Log of Imports with optional filters.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters.</param>
        /// <param name="workGroup">Optional work group filter.</param>
        /// <param name="timeCode">Optional time code (job code or test code) filter.</param>
        /// <param name="pactStaffId">Optional PACT staff ID filter.</param>
        /// <param name="parentProject">Optional parent project filter.</param>
        /// <param name="dateImported">Optional date imported filter.</param>
        /// <param name="month">Optional month number filter.</param>
        /// <param name="userId">Optional MAB user SP number filter.</param>
        /// <param name="insertDelete">Optional action (I/D/U) filter.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{MonthlyTimeLogRes}"/> containing the paged results.
        /// </returns>
        [HttpGet("log/search")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup,
            [FromQuery] string? timeCode,
            [FromQuery] string? pactStaffId,
            [FromQuery] string? parentProject,
            [FromQuery] DateTime? dateImported,
            [FromQuery] double? month,
            [FromQuery] string? userId,
            [FromQuery] string? insertDelete)
        {
            var logFilter = new Apha.PACT.Application.Dtos.MonthlyTimeLogFilterDto
            { 
                WorkGroup = workGroup, 
                TimeCode = timeCode, 
                PactStaffId = pactStaffId, 
                ParentProject = parentProject, 
                DateImported = dateImported, 
                Month = month, 
                UserId = userId, 
                InsertDelete = insertDelete };

            var result = await _service.SearchAsync(query, logFilter);

            return Ok(_mapper.Map<PaginationRes<MonthlyTimeLogRes>>(result));
        }
    }
}
