using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Workgroup lookup operations.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroups")]
    public class WorkGroupController : ControllerBase
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IMapper _mapper;

        public WorkGroupController(IWorkGroupService workGroupService, IMapper mapper)
        {
            _workGroupService = workGroupService ?? throw new ArgumentNullException(nameof(workGroupService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>Returns all WorkGroup names for dropdown population.</summary>
        [HttpGet("names")]
        public async Task<ActionResult<List<string>>> GetAllWorkGroupNamesAsync()
        {
            var result = await _workGroupService.GetAllWorkGroupNamesAsync();
            return Ok(result);
        }

        /// <summary>Returns workgroups filtered by profit centre.</summary>
        [HttpGet]
        public async Task<ActionResult<List<WorkGroupRes>>> GetWorkGroupsAsync([FromQuery] string profitCentre)
        {
            var result = await _workGroupService.GetWorkGroupsAsync(profitCentre);
            return Ok(_mapper.Map<List<WorkGroupRes>>(result));
        }
    }
}
