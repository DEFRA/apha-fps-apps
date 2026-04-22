using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    //[Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/disease")]
    [ApiController]
    [ApiVersion("1.0")]
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
