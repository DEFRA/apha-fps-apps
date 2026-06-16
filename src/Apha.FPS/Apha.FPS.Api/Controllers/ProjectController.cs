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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/project")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectController(
            IProjectService projectService,
            IMapper mapper)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{parentProject}")]
        public async Task<ActionResult<ProjectRes>> GetProjectByIdAsync(string parentProject)
        {
            var project = await _projectService.GetProjectByIdAsync(parentProject);
            if (project == null)
                throw new ArgumentException($"Project record with ID: {parentProject} not found", nameof(parentProject));
            return Ok(_mapper.Map<ProjectRes>(project));
        }



        [HttpPost]
        public async Task<ActionResult<ProjectRes>> CreateProjectAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var created = await _projectService.CreateProjectAsync(projectDto);
            var response = _mapper.Map<ProjectRes>(created);
            return CreatedAtAction(nameof(GetProjectByIdAsync), new { parentProject = response.ParentProject }, response);
        }

        [HttpPut("{parentProject}")]
        public async Task<ActionResult<ProjectRes>> UpdateProjectAsync(string parentProject, [FromBody] ProjectReq request)
        {
            if (parentProject != request.ParentProject)
                throw new ArgumentException("Route project code does not match request body.");
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdateProjectAsync(projectDto);
            return Ok(_mapper.Map<ProjectRes>(updated));
        }

        [HttpDelete("{parentProject}/delete-with-children")]
        public async Task<IActionResult> DeleteProjectAndChildrenAsync(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                throw new ArgumentException("Parent project cannot be empty.", nameof(parentProject));
            await _projectService.DeleteProjectAndChildrenAsync(parentProject);
            return Ok(true);
        }

        [HttpPost("change-code")]
        public async Task<IActionResult> ChangeProjectCodeAsync([FromBody] ChangeProjectCodeReq request)
        {
            if (string.IsNullOrWhiteSpace(request.OldCode) || string.IsNullOrWhiteSpace(request.NewCode))
                throw new ArgumentException("Both old and new project codes are required.");
            var existing = await _projectService.GetProjectByIdAsync(request.OldCode);
            if (existing == null)
                throw new ArgumentException($"Project record with code: {request.OldCode} not found");
            await _projectService.ChangeProjectCodeAsync(request.OldCode, request.NewCode);
            return Ok(true);
        }

        [HttpGet("check-exists/{code}")]
        public async Task<ActionResult<bool>> CheckProjectExistsAsync(string code)
        {
            var exists = await _projectService.CheckProjectExistsAsync(code);
            return Ok(exists);
        }

        /// <summary>
        /// Retrieves a paginated list of projects for a given programme.
        /// </summary>
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

        /// <summary>
        /// Retrieves a paginated list of all projects.
        /// </summary>
        [HttpGet("paged/all")]
        public async Task<IActionResult> GetAllProjectsPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _projectService.GetPagedProjectsAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectRes>>(result));
        }

        /// <summary>
        /// Retrieves a paginated list of projects for a given project group.
        /// </summary>
        [HttpGet("paged/by-project-group")]
        public async Task<IActionResult> GetProjectsByProjectGroupAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string projectGroup)
        {
            if (string.IsNullOrWhiteSpace(projectGroup))
                return BadRequest("projectGroup is required.");

            var result = await _projectService.GetProjectsByProjectGroupAsync(query, projectGroup);
            return Ok(_mapper.Map<PaginationRes<ProjectRes>>(result));
        }

        //Below methods moved from ProjectMaintenanceController
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

        [HttpPatch("external/pact")]
        public async Task<ActionResult<ProjectRes>> UpdatePactProjectDetailsAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdatePactProjectDetailsAsync(projectDto);
            if (updated == null)
                throw new ArgumentException($"Project record with ID: {request.ParentProject} not found");
            return Ok(_mapper.Map<ProjectRes>(updated));
        }

        [HttpPatch("external/portfolio")]
        public async Task<ActionResult<ProjectRes>> UpdatePactPortfolioDetailsAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdatePactPortfolioDetailsAsync(projectDto);
            if (updated == null)
                throw new ArgumentException($"Project record with ID: {request.ParentProject} not found");
            return Ok(_mapper.Map<ProjectRes>(updated));
        }

        [HttpPatch("external/fps-portfolio")]
        public async Task<ActionResult<ProjectRes>> UpdateFpsPortfolioDetailsAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdateFpsPortfolioDetailsAsync(projectDto);
            if (updated == null)
                throw new ArgumentException($"Project record with ID: {request.ParentProject} not found");
            return Ok(_mapper.Map<ProjectRes>(updated));
        }

        [HttpPut]
        public async Task<ActionResult<ProjectRes>> UpdateProjectRootAsync([FromBody] ProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdateProjectAsync(projectDto);
            return Ok(_mapper.Map<ProjectRes>(updated));    
        }

        [HttpDelete("{parentProject}")]
        public async Task<IActionResult> DeleteProjectAsync(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                throw new ArgumentException("Parent project cannot be empty.", nameof(parentProject));
            var deleted = await _projectService.DeleteProjectAsync(parentProject);
            if (!deleted)
                throw new ArgumentException($"Project record with ID: {parentProject} not found");
            return Ok(deleted);
        }

        /// <summary>
        /// Returns paginated project profitability rows for the given programme.
        /// workTypeFilter: "all" (default) | "approved" | "not-approved"
        /// </summary>
        [HttpGet("profitability/{programNo}")]
        public async Task<IActionResult> GetProjectProfitabilityAsync(
            [FromQuery] PaginationReq<string> query,
            string programNo,            
            [FromQuery] string workTypeFilter = "all")
        {
            if (string.IsNullOrWhiteSpace(programNo))
                throw new ArgumentException("programNo is required.");

            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _projectService.GetProjectProfitabilityAsync(filter, programNo, workTypeFilter);
            return Ok(_mapper.Map<PaginationRes<ProjectProfitabilityRes>>(result));
        }

        /// <summary>
        /// Returns paginated project profitability rows for the given project group.
        /// workTypeFilter: "all" (default) | "approved" | "not-approved"
        /// </summary>
        [HttpGet("profitability/by-project-group/{projectGroup}")]
        public async Task<IActionResult> GetProjectGroupProfitabilityAsync(
            [FromQuery] PaginationReq<string> query,
            string projectGroup,
            [FromQuery] string workTypeFilter = "all")
        {
            if (string.IsNullOrWhiteSpace(projectGroup))
                throw new ArgumentException("projectGroup is required.");

            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _projectService.GetProjectGroupProfitabilityAsync(filter, projectGroup, workTypeFilter);
            return Ok(_mapper.Map<PaginationRes<ProjectProfitabilityRes>>(result));
        }

        [HttpGet("paged/by-user")]
        public async Task<IActionResult> GetPagedProjectsByUserAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _projectService.GetPagedProjectsByUserAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectRes>>(result));
        }

        // TRANSFORMENGINE: new endpoint — frmJobcodeTotalsVLA migration
        //   Route: GET /api/v1/project/profitability-vla
        //   Filter dimensions from HTML prototype: projectStatus, programNo, manager, customer
        //   All filter fields are optional; omitting returns all rows (matches HTML "All ..." defaults).
        /// <summary>
        /// Returns a paginated list of VLA project profitability rows filtered by
        /// project status, program, manager, and/or customer.
        /// All filter parameters are optional; omitting a parameter returns all rows
        /// for that dimension (equivalent to selecting "All" in the HTML prototype dropdowns).
        /// </summary>
        /// <param name="projectStatus">Optional filter: "Approved", "Completed", or "Not Approved".</param>
        /// <param name="programNo">Optional filter by program number/name.</param>
        /// <param name="manager">Optional filter by manager name.</param>
        /// <param name="customer">Optional filter by customer name.</param>
        /// <param name="page">1-based page number (default: 1).</param>
        /// <param name="pageSize">Rows per page (default: 15, max: 100).</param>
        [HttpGet("profitability-vla")]
        public async Task<IActionResult> GetProjectProfitabilityVlaAsync(
            [FromQuery] string? projectStatus = null,
            [FromQuery] string? programNo = null,
            [FromQuery] string? manager = null,
            [FromQuery] string? customer = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15)
        {
            // TRANSFORMENGINE: build QueryParameters<ProjectProfitabilityVlaReq> from individual
            //   query-string params rather than binding the whole Req object via [FromQuery] to
            //   keep the public route contract flat and consistent with how the HTML prototype JS
            //   constructs the GET request (individual named params on the URL).

            // TRANSFORMENGINE [Phase 14 security fix]: clamp pagination params to safe bounds.
            //   Flat int bindings carry no [Range] annotation so client could pass arbitrary values.
            //   Consistent with [Range(1,100)] on ProjectProfitabilityVlaReq.PageSize (Phase 1 contract).
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            var query = new QueryParameters<ProjectProfitabilityVlaReq>
            {
                Page = page,
                PageSize = pageSize,
                Filter = new ProjectProfitabilityVlaReq
                {
                    ProjectStatus = projectStatus,
                    ProgramNo = programNo,
                    Manager = manager,
                    Customer = customer,
                    Page = page,
                    PageSize = pageSize
                }
            };

            var result = await _projectService.GetProjectProfitabilityVlaAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(result));
        }
    }

    public record ChangeProjectCodeReq(string OldCode, string NewCode);
}

