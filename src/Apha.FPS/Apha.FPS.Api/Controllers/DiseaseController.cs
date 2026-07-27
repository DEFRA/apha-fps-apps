using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/disease")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DiseaseController : ControllerBase
    {
        private readonly IDiseaseService _diseaseService;
        private readonly IMapper _mapper;

        public DiseaseController(IDiseaseService diseaseService, IMapper mapper)
        {
            _diseaseService = diseaseService ?? throw new ArgumentNullException(nameof(diseaseService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDiseasesAsync()
        {
            var diseases = await _diseaseService.GetAllDiseasesAsync();
            return Ok(_mapper.Map<List<DiseaseRes>>(diseases));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DiseaseReq req)
        {
            var dto = _mapper.Map<DiseaseDto>(req);
            var result = await _diseaseService.CreateDiseaseAsync(dto);
            return CreatedAtAction(nameof(GetAllDiseasesAsync), _mapper.Map<DiseaseRes>(result));
        }

        [HttpDelete("{diseaseName}")]
        public async Task<IActionResult> DeleteAsync(string diseaseName)
        {
            var isDeleted = await _diseaseService.DeleteDiseaseAsync(diseaseName);
            if (!isDeleted)
                throw new KeyNotFoundException("Disease not found.");
            return Ok(isDeleted);
        }
    }
}
