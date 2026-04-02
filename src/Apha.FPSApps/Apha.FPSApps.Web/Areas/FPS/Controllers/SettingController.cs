using Apha.FPSApps.Application.Interfaces.FPS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;

        public SettingController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHoursPerDay()
        {
            var result = await _settingService.GetHoursPerDayAsync();

            if (result.Success)
                return Json(new { success = true, hoursPerDay = result.Data });

            return Json(new { success = false, hoursPerDay = 8.0 });
        }
    }
}
