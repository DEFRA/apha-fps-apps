using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Components
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationModel pagination)
        {
            return View(pagination);
        }
    }
}
