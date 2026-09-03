using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for managing FPS settings.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/setting")]
    public class FpsSettingController : ControllerBase
    {
        private readonly IFpsSettingService _fpsSettingService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FpsSettingController"/> class.
        /// </summary>
        /// <param name="fpsSettingService">The FPS setting service.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        public FpsSettingController(
                        IFpsSettingService fpsSettingService,
                        IMapper mapper)
        {
            _fpsSettingService = fpsSettingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all FPS settings.
        /// </summary>
        /// <returns>A list of FPS settings.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _fpsSettingService.GetAllSettingsAsync();
            return Ok(_mapper.Map<List<FpsSettingRes>>(result));
        }

        /// <summary>
        /// Gets the configured number of working hours per day.
        /// </summary>
        /// <returns>The hours per day value.</returns>
        [HttpGet("hoursperday")]
        public async Task<IActionResult> GetHoursPerDayAsync()
        {
            var result = await _fpsSettingService.GetHoursPerDayAsync();
            return Ok(result);
        }

        /// <summary>
        /// Gets year-end settings. With <paramref name="jobExecutionId"/>: current/Open-year values
        /// overlaid with that Data Setup request's staged rows (planned-year staging design).
        /// Without it: legacy YearMasters-status-driven behavior, unchanged — kept for callers that
        /// don't yet supply a JobExecutionId (FPSApps page-load path; optional until Workstream 8
        /// finishes migrating every caller, at which point this becomes required).
        /// </summary>
        /// <param name="jobExecutionId">The Data Setup request to load settings for, if known.</param>
        /// <returns>A list of year-end settings.</returns>
        [HttpGet("yearend")]
        public async Task<IActionResult> GetYearEndSettingsAsync([FromQuery] Guid? jobExecutionId = null)
        {
            var result = jobExecutionId.HasValue
                ? await _fpsSettingService.GetYearEndSettingsAsync(jobExecutionId.Value)
                : await _fpsSettingService.GetYearEndSettingsAsync();
            return Ok(_mapper.Map<List<FpsYearEndSettingRes>>(result));
        }

        /// <summary>
        /// Creates a new FPS setting.
        /// </summary>
        /// <param name="request">The setting to create.</param>
        /// <returns>The created FPS setting.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] FpsSettingReq request)
        {
            var dto = _mapper.Map<FpsSettingDto>(request);
            var result = await _fpsSettingService.AddSettingAsync(dto);
            return CreatedAtAction(nameof(GetAsync), _mapper.Map<FpsSettingRes>(result));
        }

        /// <summary>
        /// Updates an existing FPS setting.
        /// </summary>
        /// <param name="id">The key of the setting to update.</param>
        /// <param name="request">The updated setting values.</param>
        /// <returns>The updated FPS setting.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(string id, [FromBody] FpsSettingReq request)
        {
            request.Id = id;
            var dto = _mapper.Map<FpsSettingDto>(request);
            var result = await _fpsSettingService.UpdateSettingAsync(dto);
            return Ok(_mapper.Map<FpsSettingRes>(result));
        }

        /// <summary>
        /// Confirms a year-end Config Value for a specific Data Setup request (planned-year staging
        /// design) — upserts a staged row, never writes fps.tblsettings directly. Requires the
        /// request to still be Initiated.
        /// </summary>
        /// <param name="jobExecutionId">The Data Setup request this value is being confirmed for.</param>
        /// <param name="request">The setting to confirm.</param>
        /// <returns>The confirmed value.</returns>
        [HttpPost("save")]
        public async Task<IActionResult> SaveAsync([FromQuery] Guid jobExecutionId, [FromBody] FpsSettingReq request)
        {
            var dto = _mapper.Map<FpsSettingDto>(request);
            var result = await _fpsSettingService.SaveSettingAsync(jobExecutionId, dto);
            return Ok(_mapper.Map<FpsSettingRes>(result));
        }
    }
}
