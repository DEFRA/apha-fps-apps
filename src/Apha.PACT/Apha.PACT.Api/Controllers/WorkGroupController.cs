using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [AllowAnonymous]
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllWorkGroupsAsync();
            return Ok(_mapper.Map<IEnumerable<WorkGroupRes>>(items));
        }
    }
}
