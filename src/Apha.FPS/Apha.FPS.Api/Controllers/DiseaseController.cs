using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/disease")]
    [ApiController]
    public class DiseaseController : ControllerBase
    {
        private readonly IDiseaseService _diseaseService;

        public DiseaseController(IDiseaseService diseaseService)
        {
            _diseaseService = diseaseService ?? throw new ArgumentNullException(nameof(diseaseService));
        }

        [HttpGet]
        public async Task<ActionResult<List<DiseaseRes>>> GetAllDiseasesAsync()
        {
            var diseases = await _diseaseService.GetAllDiseasesAsync();
            return Ok(diseases.Select(d => new DiseaseRes { Disease = d }).ToList());
        }
    }
}
