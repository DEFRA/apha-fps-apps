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
    /// API controller for managing project data.
    /// </summary>
   // [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/project")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectController"/> class.
        /// </summary>
        /// <param name="projectService">Service for project operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public ProjectController(IProjectService projectService, IMapper mapper)
        {
            _projectService = projectService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of projects for a given programme.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="programNo">The programme number to filter projects by.</param>
        /// <returns>Paginated list of project results.</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetProjectsByProgramAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
                return BadRequest("programNo is required.");

            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);
            return Ok(_mapper.Map<PaginationRes<ProjectRes>>(result));
        }
    }
}
