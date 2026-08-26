using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult AccessDenied(string? returnUrl)
        {
            // If access was denied while trying to reach an FPS page,
            // forward to the FPS-styled access denied page.
            if (!string.IsNullOrEmpty(returnUrl) &&
                returnUrl.StartsWith("/FPS/", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account",
                    new { area = "FPS", returnUrl });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
