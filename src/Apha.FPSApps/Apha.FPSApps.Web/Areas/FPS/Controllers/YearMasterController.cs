using Apha.FPSApps.Application.Interfaces.FPS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class YearMasterController : Controller
    {
        private readonly IYearMasterService _yearMasterService;

        public YearMasterController(IYearMasterService yearMasterService)
        {
            _yearMasterService = yearMasterService;
        }

        /// <summary>
        /// Get year masters as dropdown list for layout and other forms
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetYearMasterDropdown()
        {
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            if (result.Success && result.Data != null)
            {
                var selectList = result.Data
                    .Where(y => y.Active) // Only active years
                    .OrderByDescending(y => y.FpsYear)
                    .Select(y => new
                    {
                        value = y.FpsYear,
                        text = y.FpsYearCode,
                        yearStatus = y.YearStatus
                    })
                    .ToList();

                return Json(new { success = true, data = selectList });
            }

            return Json(new { success = false, message = "Failed to retrieve year masters" });
        }
    }
}
