using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Components
{
    public class AppNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string appArea)
        {
            return View((object)appArea);
        }
    }
}
