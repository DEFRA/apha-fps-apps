using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for Month operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/months")]
    public class MonthController : ControllerBase
    {
        private readonly IMonthService _service;
        private readonly IMapper _mapper;

        public MonthController(IMonthService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all months.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<MonthDto> months = await _service.GetAllMonthsAsync();
            return Ok(_mapper.Map<IEnumerable<MonthRes>>(months));
        }
    }
}
