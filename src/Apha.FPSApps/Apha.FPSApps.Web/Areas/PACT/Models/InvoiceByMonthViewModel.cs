using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class InvoiceByMonthViewModel
    {
        public DataGridConfig<MonthlyInvoicePivotRow> Grid { get; set; } = new DataGridConfig<MonthlyInvoicePivotRow>();
    }
}
