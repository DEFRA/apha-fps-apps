using Apha.Common.Contracts;
using Apha.Costbook.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Settings")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        [HttpGet("getvaluebyid")]
        public async Task<IActionResult> GetSettingValueByIdAsync([FromQuery] string? id)
        {
            var number = await _settingsService.GetSettingValueByIdAsync(id);
            var response = new ApiResponse<string>
            {
                Success = true,
                Data = number,
                Errors = new List<ApiError>(),
                Meta = new ApiMeta()
            };
            return Ok(response);
        }
    }
}
