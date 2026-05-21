using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/profitcentre")]
    public class ProfitCentreController : ControllerBase
    {
        private readonly IProfitCentreService _service;
        private readonly IMapper _mapper;

        public ProfitCentreController(IProfitCentreService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all profit centres from the <c>vPacttblkpProfitCentre</c> view, including
        /// their associated timesheet, output-sheet, and layout settings.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProfitCentres()
        {
            var items = await _service.GetAllProfitCentresAsync();
            return Ok(_mapper.Map<IEnumerable<ProfitCentreSettingsRes>>(items));
        }

        /// <summary>
        /// Returns the timesheet, output-sheet, and timesheet-layout settings for the specified
        /// profit centre. Returns <c>404 Not Found</c> if no matching record exists.
        /// </summary>
        /// <param name="profitCentre">The profit-centre code to retrieve settings for.</param>
        [HttpGet("{profitCentre}/settings")]
        public async Task<IActionResult> GetSettings(string profitCentre)
        {
            var settings = await _service.GetProfitCentreSettingsAsync(profitCentre);
            if (settings == null)
                return NotFound();

            return Ok(_mapper.Map<ProfitCentreSettingsRes>(settings));
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

            var success = await _service.UpdateProfitCentreSettingsAsync(
                request.ProfitCentre,
                request.Timesheet,
                request.Outputsheet,
                request.TimesheetLayout);

            return Ok(success);
        }
    }
}
