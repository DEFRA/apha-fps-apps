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
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/monthhour")]
    public class MonthHourController : ControllerBase
    {
        private readonly IMonthHourService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="MonthHourController"/>.
        /// </summary>
        /// <param name="service">Month-hour application service.</param>
        /// <param name="mapper">AutoMapper instance.</param>
        public MonthHourController(IMonthHourService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves paged month-hour records with optional filtering and ordering.
        /// Supports filtering by <c>Year</c> and <c>Month</c> through the query filter payload,
        /// and ordering by <c>Year</c>, <c>Month</c>, <c>Days</c>, <c>CvlHours</c>, or <c>VidHours</c>.
        /// </summary>
        /// <param name="query">Pagination, filter and sort options.</param>
        /// <returns><c>200 OK</c> with a <see cref="PaginationRes{MonthHourRes}"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(_mapper.Map<PaginationRes<MonthHourRes>>(result));
        }

        /// <summary>
        /// Retrieves all month-hour records for the specified <paramref name="year"/>.
        /// </summary>
        /// <param name="year">The financial year to filter by.</param>
        /// <returns><c>200 OK</c> with an <see cref="IEnumerable{MonthHourRes}"/>.</returns>
        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear(short year)
        {
            var items = await _service.GetByYearAsync(year);
            return Ok(_mapper.Map<IEnumerable<MonthHourRes>>(items));
        }

        /// <summary>
        /// Returns the distinct list of years present in the month-hour table, suitable for
        /// populating the year drop-down on the COS90 page.
        /// </summary>
        /// <returns><c>200 OK</c> with an <see cref="IEnumerable{short}"/> of years.</returns>
        [HttpGet("years")]
        public async Task<IActionResult> GetDistinctYears()
        {
            var years = await _service.GetDistinctYearsAsync();
            return Ok(years);
        }
    }
}
