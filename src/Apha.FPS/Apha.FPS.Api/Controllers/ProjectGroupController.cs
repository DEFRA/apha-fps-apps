using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for retrieving project group lookup data.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/projectgroup")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProjectGroupController : ControllerBase
    {
        private readonly IProjectGroupService _projectGroupService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectGroupController"/> class.
        /// </summary>
        /// <param name="projectGroupService">Service for project group lookup.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public ProjectGroupController(IProjectGroupService projectGroupService, IMapper mapper)
        {
            _projectGroupService = projectGroupService ?? throw new ArgumentNullException(nameof(projectGroupService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Retrieves all project groups.
        /// </summary>
        /// <returns>List of project groups.</returns>
        [HttpGet]
        public async Task<ActionResult<List<ProjectGroupRes>>> GetAllProjectGroupsAsync()
        {
            var projectGroups = await _projectGroupService.GetAllProjectGroupsAsync();
            return Ok(_mapper.Map<List<ProjectGroupRes>>(projectGroups));
        }

        /// <summary>
        /// Retrieves project groups filtered by the current user.
        /// </summary>
        /// <returns>List of project groups for the current user.</returns>
        [HttpGet("by-user")]
        public async Task<ActionResult<List<ProjectGroupRes>>> GetProjectGroupsByUserAsync()
        {
            var projectGroups = await _projectGroupService.GetAllProjectGroupsByUserAsync();
            return Ok(_mapper.Map<List<ProjectGroupRes>>(projectGroups));
        }
    }
}
