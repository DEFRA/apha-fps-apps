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
    //  [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/project")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProjectMaintenanceController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectMaintenanceController(IProjectService projectService, IMapper mapper)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<List<ProjectRes>>> GetAllProjectsAsync()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(_mapper.Map<List<ProjectRes>>(projects));
        }        

        [HttpGet("pactview")]
        public async Task<ActionResult<PaginationRes<ProjectRes>>> GetPagedPactProjectsAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var pagedResult = await _projectService.GetPagedPactProjectsAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectRes>>(pagedResult));
        }

        [HttpGet("pactview/all")]
        public async Task<ActionResult<List<ProjectRes>>> GetAllPactProjectsAsync()
        {
            var projects = await _projectService.GetAllPactProjectsAsync();
            return Ok(_mapper.Map<List<ProjectRes>>(projects));
        }

        [HttpGet("{parentProject}")]
        public async Task<ActionResult<ProjectRes>> GetProjectByIdAsync(string parentProject)
        {
            var project = await _projectService.GetProjectByIdAsync(parentProject);
            if (project == null)
                return NotFound();
            return Ok(_mapper.Map<ProjectRes>(project));
        }

        [HttpPost]
        public async Task<ActionResult<ProjectRes>> CreateProjectAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var created = await _projectService.CreateProjectAsync(projectDto);
            return CreatedAtAction(nameof(GetProjectByIdAsync),
                new { parentProject = created.ParentProject },
                _mapper.Map<ProjectRes>(created));
        }

        [HttpPut]
        public async Task<ActionResult<ProjectRes>> UpdateProjectAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdateProjectAsync(projectDto);
            return Ok(_mapper.Map<ProjectRes>(updated));
        }

        [HttpPatch("external/pact")]
        public async Task<ActionResult<ProjectRes>> UpdatePactProjectDetailsAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdatePactProjectDetailsAsync(projectDto);
            if (updated == null)
                return NotFound();
            return Ok(_mapper.Map<ProjectRes>(updated));
        }

        [HttpDelete("{parentProject}")]
        public async Task<IActionResult> DeleteProjectAsync(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                return BadRequest("Parent project cannot be empty.");
            var deleted = await _projectService.DeleteProjectAsync(parentProject);
            if (!deleted)
                return NotFound();
            return Ok(deleted);
        }        
    }
}
