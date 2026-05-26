using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class AutomaticMonthlyInvoiceViewModel
    {
        public int? SelectedMonth { get; set; }
        public DataGridConfig<AutomaticInvoiceItem> InvoicesGrid { get; set; } = new DataGridConfig<AutomaticInvoiceItem>();
        public List<SelectListItem> Months { get; set; } = new List<SelectListItem>();
    }
}
