using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for CalenderMonth operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/calendermonth")]
    public class CalenderMonthController : ControllerBase
    {
        private readonly ICalenderMonthService _service;
        private readonly IMapper _mapper;

        public CalenderMonthController(ICalenderMonthService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all calendar months.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCalenderMonthsAsync()
        {
            var items = await _service.GetCalenderMonthsAsync();
            return Ok(_mapper.Map<IEnumerable<CalenderMonthRes>>(items));
        }
    }
}
