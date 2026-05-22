using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for profit centres used to populate the Resource Centre dropdown.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/profitcentres")]
    public class ProfitCentreController : ControllerBase
    {
        private readonly IProfitCentreService _profitCentreService;
        private readonly IMapper _mapper;

        public ProfitCentreController(IProfitCentreService profitCentreService, IMapper mapper)
        {
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns all profit centres for the Resource Centre dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            return Ok(_mapper.Map<List<ProfitCentreRes>>(result));
        }
    }
}
