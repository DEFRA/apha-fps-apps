    using Apha.FPSApps.Web.Models.Components.AppHeader;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apha.FPSApps.Web.Components
{
    public class AppHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string appName, bool showYearSelector = false)
        {
            var displayName = HttpContext.User.FindFirst("name")?.Value
                           ?? HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value
                           ?? HttpContext.User.FindFirst(ClaimTypes.Name)?.Value?.Split('@')[0]
                           ?? "Unknown User";

            return View(new AppHeaderViewModel
            {
                AppName = appName,
                ShowYearSelector = showYearSelector,
                DisplayName = displayName
            });
        }
    }
}
