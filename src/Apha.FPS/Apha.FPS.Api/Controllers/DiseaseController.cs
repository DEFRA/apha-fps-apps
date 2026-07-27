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

        [HttpGet("{diseaseName}")]
        public async Task<IActionResult> GetByNameAsync(string diseaseName)
        {
            var disease = await _diseaseService.GetDiseaseByNameAsync(diseaseName);
            if (disease == null)
                throw new KeyNotFoundException("Disease not found.");
            return Ok(_mapper.Map<DiseaseRes>(disease));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DiseaseReq req)
        {
            var dto = _mapper.Map<DiseaseDto>(req);
            var result = await _diseaseService.CreateDiseaseAsync(dto);
            var res = _mapper.Map<DiseaseRes>(result);
            return CreatedAtAction(nameof(GetByNameAsync), new { diseaseName = result.DiseaseName }, res);
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
