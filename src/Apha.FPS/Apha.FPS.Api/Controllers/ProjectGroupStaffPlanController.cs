using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Project Group Staff Plan pivot view (fps.vpvtprojectgroupmgrplan).
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectgroupstaffplan")]
    public class ProjectGroupStaffPlanController : ControllerBase
    {
        private readonly IProjectGroupStaffPlanService _service;
        private readonly IMapper _mapper;

        public ProjectGroupStaffPlanController(IProjectGroupStaffPlanService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paginated, filterable list of staff plan records from fps.vpvtprojectgroupmgrplan.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query)
        {
            PaginatedResult<ProjectGroupStaffPlanViewDto> result = await _service.GetPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectGroupStaffPlanViewRes>>(result));
        }
    }
}
