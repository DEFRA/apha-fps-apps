using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class InvoiceViewModel
    {
        public string ParentProject { get; set; } = string.Empty;
        public int? Month { get; set; }
        public DataGridConfig<InvoiceItem> InvoicesGrid { get; set; } = new DataGridConfig<InvoiceItem>();
        public List<SelectListItem> FilterProjects { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FilterMonths { get; set; } = new List<SelectListItem>();
    }
}
