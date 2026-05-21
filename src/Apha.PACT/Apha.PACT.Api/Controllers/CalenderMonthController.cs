using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for calendar month lookup operations.
    /// Exposes endpoints under <c>api/v{version}/calendermonth</c> and requires
    /// the <c>API-PACTUser</c> or <c>API-PACTAdmin</c> role.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/calendermonth")]
    public class CalenderMonthController : ControllerBase
    {
        private readonly ICalenderMonthService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="CalenderMonthController"/> with the required
        /// calendar month service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve calendar month data.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public CalenderMonthController(ICalenderMonthService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all calendar months available in the system.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{CalenderMonthRes}"/> containing all calendar months
        /// ordered as returned by the underlying service.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetCalenderMonthsAsync()
        {
            var items = await _service.GetCalenderMonthsAsync();
            return Ok(_mapper.Map<IEnumerable<CalenderMonthRes>>(items));
        }
    }
}
