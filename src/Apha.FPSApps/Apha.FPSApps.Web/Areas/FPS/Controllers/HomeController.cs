using Apha.FPSApps.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    // [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AllowAnonymous]
    //[AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")] // COMMENTED OUT FOR LOCAL DEVELOPMENT
    public class HomeController : Controller
    {
        public HomeController()
        {            
        }

        public IActionResult Index()
        {  
            return View();
        }
    }
}
