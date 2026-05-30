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
    [Route("api/v{version:apiVersion}/monthlyoutput")]
    public class MonthlyOutputController : ControllerBase
    {
        private readonly IMonthlyOutputService _service;
        private readonly IMapper _mapper;

        public MonthlyOutputController(IMonthlyOutputService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("log/search")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup,
            [FromQuery] string? testCode,
            [FromQuery] string? buyer,
            [FromQuery] DateTime? dateImported,
            [FromQuery] double? month,
            [FromQuery] string? userId,
            [FromQuery] string? insertDelete)
        {
            var result = await _service.GetMonthlyOutputLogAsync(
                query, workGroup, testCode, buyer, dateImported, month, userId, insertDelete);

            return Ok(_mapper.Map<PaginationRes<MonthlyOutputLogRes>>(result));
        }
    }
}
