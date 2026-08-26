using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        /// <summary>
        /// Renders the FPS-styled access denied page.
        /// The ReturnUrl is passed to the view so the Back button can navigate
        /// the user to the page they originally tried to access.
        /// </summary>
        public IActionResult AccessDenied(string? returnUrl)
        {
            return View(model: returnUrl);
        }
    }
}
