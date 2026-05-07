using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    /// <summary>
    /// API controller for Project Month (Cost Profile Grid) operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectmonth")]
    public class ProjectMonthController : ControllerBase
    {
        private readonly IProjectMonthService _service;
        private readonly IMapper _mapper;

        public ProjectMonthController(IProjectMonthService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all months (accounting period and month name lookup).</summary>
        [HttpGet("months")]
        public async Task<IActionResult> GetMonths()
        {
            IList<MonthDto> items = await _service.GetMonthsAsync();
            return Ok(_mapper.Map<IList<MonthRes>>(items));
        }

        /// <summary>Retrieves all cost profile months for a given project.</summary>
        [HttpGet("project/{project}")]
        public async Task<IActionResult> GetProjectMonthByProject(string project)
        {
            IList<ProjectMonthDto> items = await _service.GetProjectMonthByProjectAsync(project);
            return Ok(_mapper.Map<IList<ProjectMonthRes>>(items));
        }

        /// <summary>Retrieves a single cost profile month record by project and month number.</summary>
        [HttpGet("project/{project}/month/{monthNo:int}")]
        public async Task<IActionResult> GetProjectMonth(string project, int monthNo)
        {
            ProjectMonthDto? item = await _service.GetProjectMonthAsync(project, monthNo);
            if (item is null)
                throw new KeyNotFoundException($"Project month record not found for project '{project}', month {monthNo}.");
            return Ok(_mapper.Map<ProjectMonthRes>(item));
        }

        /// <summary>Creates a new cost profile month record.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateProjectMonth([FromBody] ProjectMonthReq request)
        {
            ProjectMonthDto dto = _mapper.Map<ProjectMonthDto>(request);
            ProjectMonthDto created = await _service.CreateProjectMonthAsync(dto);
            return CreatedAtAction(
                nameof(GetProjectMonth),
                new { project = created.Project, monthNo = created.MonthNo },
                _mapper.Map<ProjectMonthRes>(created));
        }

        /// <summary>Updates an existing cost profile month record.</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProjectMonth([FromBody] ProjectMonthReq request)
        {
            ProjectMonthDto dto = _mapper.Map<ProjectMonthDto>(request);
            ProjectMonthDto updated = await _service.UpdateProjectMonthAsync(dto);
            return Ok(_mapper.Map<ProjectMonthRes>(updated));
        }

        /// <summary>Deletes a cost profile month record.</summary>
        [HttpDelete("project/{project}/month/{monthNo:int}")]
        public async Task<IActionResult> DeleteProjectMonth(string project, int monthNo)
        {
            bool deleted = await _service.DeleteProjectMonthAsync(project, monthNo);
            if (!deleted)
                return NotFound();
            return Ok(deleted);
        }
    }
}
