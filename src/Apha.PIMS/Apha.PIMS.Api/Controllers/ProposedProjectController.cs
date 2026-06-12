using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/proposedproject")]
    public class ProposedProjectController : ControllerBase
    {
        private readonly IProposedProjectService _service;
        private readonly IMapper _mapper;

        public ProposedProjectController(IProposedProjectService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("{parentproject}")]
        public async Task<IActionResult> GetProposedProjectById(string parentproject)
        {
            ProposedProjectDto? result = await _service.GetProposedProjectByIdAsync(parentproject);
            return Ok(_mapper.Map<ProposedProjectRes>(result));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProposedProject([FromBody] ProposedProjectReq request)
        {
            ProposedProjectDto dto = _mapper.Map<ProposedProjectDto>(request);
            ProposedProjectDto result = await _service.AddProposedProjectAsync(dto);
            return CreatedAtAction(nameof(GetProposedProjectById),
                new { parentproject = result.Parentproject },
                _mapper.Map<ProposedProjectRes>(result));
        }

        [HttpGet("programs")]
        public async Task<IActionResult> GetProjectPrograms()
        {
            List<string> result = await _service.GetProjectProgramsAsync();
            return Ok(result);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetProjectCustomers()
        {
            List<string> result = await _service.GetProjectCustomersAsync();
            return Ok(result);
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetProjectStatuses()
        {
            List<string> result = await _service.GetProjectStatusesAsync();
            return Ok(result);
        }
    }
}
