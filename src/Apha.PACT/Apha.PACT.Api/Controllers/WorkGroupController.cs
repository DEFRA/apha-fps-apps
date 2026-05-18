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

        public WorkGroupController(IWorkGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all WorkGroups.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllWorkGroupsAsync();
            return Ok(_mapper.Map<IEnumerable<WorkGroupRes>>(items));
        }

        /// <summary>Retrieves a paged and sorted list of WorkGroup time codes.</summary>
        [HttpGet("paged/timecodes")]
        public async Task<IActionResult> GetPagedWorkGroupTimeCodes(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup = null,
            [FromQuery] int? monthNumber = null)
        {
            var result = await _service.GetWorkGroupTimeCodeAsync(query, workGroup, monthNumber);
            return Ok(_mapper.Map<PaginationRes<WorkGroupTimeCodeRes>>(result));
        }
    }
}
