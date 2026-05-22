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
        /// Returns all user-specific profit centres for the Resource Centre dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            return Ok(_mapper.Map<List<ProfitCentreRes>>(result));
        }

        /// <summary>
        /// Returns all profit centres including their associated timesheet, output-sheet, and layout settings.         
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProfitCentres()
        {
            var items = await _profitCentreService.GetAllProfitCentresAsync();
            return Ok(_mapper.Map<IEnumerable<ProfitCentreRes>>(items));
        }

        /// <summary>
        /// Returns the timesheet, output-sheet, and timesheet-layout settings for the specified
        /// profit centre. Returns <c>404 Not Found</c> if no matching record exists.
        /// </summary>
        /// <param name="profitCentre">The profit-centre code to retrieve settings for.</param>
        [HttpGet("{profitCentre}")]
        public async Task<IActionResult> GetProfitCentreById(string profitCentre)
        {
            var settings = await _profitCentreService.GetProfitCentreByIdAsync(profitCentre);
            if (settings == null)
                return NotFound();

            return Ok(_mapper.Map<ProfitCentreRes>(settings));
        }

        /// <summary>
        /// Partially updates the timesheet, output-sheet, and timesheet-layout settings for the
        /// specified profit centre. Only the three settings fields are written; other profit-centre
        /// data is left unchanged.
        /// </summary>
        /// <param name="request">Contains the profit-centre code and the new values for
        /// <c>Timesheet</c>, <c>Outputsheet</c>, and <c>TimesheetLayout</c>.</param>
        [HttpPatch("settings")]
        public async Task<IActionResult> PatchSettings([FromBody] UpdateProfitCentreSettingsReq request)
        {
            if (string.IsNullOrWhiteSpace(request.ProfitCentre))
                return BadRequest("ProfitCentre is required.");

            var success = await _profitCentreService.UpdateProfitCentreSettingsAsync(
                request.ProfitCentre,
                request.Timesheet,
                request.Outputsheet,
                request.TimesheetLayout);

            return Ok(success);
        }
    }
}
