using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractByMonthViewModel
    {
        public DataGridConfig<SubContractByMonthPivotRow> Grid { get; set; } = new DataGridConfig<SubContractByMonthPivotRow>();
    }
}
