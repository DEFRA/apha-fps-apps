using Apha.FPSApps.Application.Interfaces.Costbook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class SettingsController : Controller
    {
        private readonly ICostBookSettingsService _settingService;

        public SettingsController(ICostBookSettingsService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]

        public async Task<IActionResult> GetSettingValueById(string id)
        {
            var result = await _settingService.GetSettingValueByIdAsync(id);

            if (result.Success)
                return Json(new { success = true, hoursPerDay = result.Data });

            return Json(new { success = false, hoursPerDay = 7.2 });
        }
        }

        
    }


