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
    /// <summary>
    /// API controller for TestActualBreakdown operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testactualbreakdown")]
    public class TestActualBreakdownController : ControllerBase
    {
        private readonly ITestActualBreakdownService _service;
        private readonly IMapper _mapper;

        public TestActualBreakdownController(ITestActualBreakdownService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Returns a paged list of rows from fps.vqryTestsActualBreakdown.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<TestActualBreakdownRes>>(result));
        }
    }
}
