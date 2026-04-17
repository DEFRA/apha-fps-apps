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
    [Route("api/v{version:apiVersion}/projectdetails")]
    public class ProjectDetailsController : ControllerBase
    {
        private readonly IProjectDetailsService _service;
        private readonly IMapper _mapper;

        public ProjectDetailsController(IProjectDetailsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        
        [HttpGet("{parentproject}/pims")]
        public async Task<IActionResult> GetPimsDetail(string parentproject)
        {
            ProjectDetailDto? result = await _service.GetPimsDetailAsync(parentproject);            
            return Ok(_mapper.Map<ProjectDetailRes>(result));
        }

       
        [HttpPost("{parentproject}/pims")]
        public async Task<IActionResult> SavePimsDetail(string parentproject, [FromBody] ProjectDetailReq request)
        {
            ProjectDetailDto dto = _mapper.Map<ProjectDetailDto>(request);
            dto.Parentproject = parentproject;
            ProjectDetailDto result = await _service.SavePimsDetailAsync(dto);
            return Ok(_mapper.Map<ProjectDetailRes>(result));
        }

       
        [HttpGet("{parentproject}/proposed")]
        public async Task<IActionResult> GetProposedProject(string parentproject)
        {
            ProposedProjectDto? result = await _service.GetProposedProjectAsync(parentproject);            
            return Ok(_mapper.Map<ProposedProjectRes>(result));
        }

        
        [HttpPut("{parentproject}/proposed")]
        public async Task<IActionResult> UpdateProposedProject(string parentproject, [FromBody] ProposedProjectReq request)
        {
            ProposedProjectDto dto = _mapper.Map<ProposedProjectDto>(request);
            dto.Parentproject = parentproject;
            ProposedProjectDto result = await _service.UpdateProposedProjectAsync(dto);
            return Ok(_mapper.Map<ProposedProjectRes>(result));
        }

        [HttpGet("risks")]
        public async Task<IActionResult> GetAllRisk()
        {
            List<RiskDto> result = await _service.GetAllRiskAsync();
            return Ok(_mapper.Map<List<RiskRes>>(result));
        }
    }
}
