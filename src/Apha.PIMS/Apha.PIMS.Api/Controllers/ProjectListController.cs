using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{    
     
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectlist")]
    public class ProjectListController : ControllerBase
    {
        private readonly IProjectListService _service;
        private readonly IMapper _mapper;

        public ProjectListController(IProjectListService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllProjectsAsync([FromQuery] PaginationReq<string> query, [FromQuery] int showWhichProjects = 2)
        {
            QueryParameters<string> filter = _mapper.Map<QueryParameters<string>>(query);
            PaginatedResult<ProjectListViewDto> result = await _service.GetAllProjectsAsync(filter, showWhichProjects);
            return Ok(_mapper.Map<PaginationRes<ProjectListRes>>(result));
        }

        [HttpGet("AllProjectsList")]
        public async Task<IActionResult> GetAllProjectsForDropDownAsync()
        {
            List<ProjectListViewDto> result = await _service.GetAllProjectsForDropDownAsync();
            return Ok(_mapper.Map<List<ProjectListRes>>(result));
        }


       
       
        [HttpGet("{parentproject}/fps")]
        public async Task<IActionResult> GetFpsProjectById(string parentproject)
        {
            ProjectDto? result = await _service.GetFpsProjectByIdAsync(parentproject);
            if (result is null)
                throw new KeyNotFoundException($"FPS project '{parentproject}' not found.");
            return Ok(_mapper.Map<ProjectRes>(result));
        }

        
        [HttpGet("{parentproject}/proposed")]
        public async Task<IActionResult> GetProposedProjectById(string parentproject)
        {
            ProposedProjectDto? result = await _service.GetProposedProjectByIdAsync(parentproject);
            if (result is null)
                throw new KeyNotFoundException($"Proposed project '{parentproject}' not found.");
            return Ok(_mapper.Map<ProposedProjectRes>(result));
        }

       
        [HttpGet("{parentproject}/yearly")]
        public async Task<IActionResult> GetYearlyDetailsByProject(string parentproject)
        {
            List<ProjectsDto> result = await _service.GetYearlyDetailsByProjectAsync(parentproject);
            return Ok(_mapper.Map<List<ProjectsRes>>(result));
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProposedProjectReq request)
        {
            ProposedProjectDto dto = _mapper.Map<ProposedProjectDto>(request);
            ProposedProjectDto result = await _service.AddProjectAsync(dto);
            return CreatedAtAction(nameof(GetProposedProjectById),
                new { parentproject = result.Parentproject },
                _mapper.Map<ProposedProjectRes>(result));
        }
    }
}
