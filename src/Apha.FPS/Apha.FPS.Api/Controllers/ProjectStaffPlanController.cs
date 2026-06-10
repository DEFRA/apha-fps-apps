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
    /// API controller for the Project Staff Plan summary view (fps.vprojectstaffplan).
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectstaffplan")]
    public class ProjectStaffPlanController : ControllerBase
    {
        private readonly IProjectStaffPlanService _service;
        private readonly IMapper _mapper;

        public ProjectStaffPlanController(IProjectStaffPlanService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paginated, filterable list of staff plan records from fps.vprojectstaffplan.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query)
        {
            PaginatedResult<ProjectStaffPlanViewDto> result = await _service.GetPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectStaffPlanViewRes>>(result));
        }
    }
}
