using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Components
{
    public class AppFooterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
