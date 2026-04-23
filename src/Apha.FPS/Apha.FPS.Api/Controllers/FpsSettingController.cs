using Apha.Common.Contracts.FPS;
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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
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
    }
}
