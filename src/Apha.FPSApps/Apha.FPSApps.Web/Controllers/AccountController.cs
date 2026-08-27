using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult AccessDenied(string? returnUrl)
        {
            return View();
        }
    }
}
