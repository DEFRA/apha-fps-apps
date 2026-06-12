using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
   
    public class ProfitCenterCostSummaryViewModel
    {
        public short? SelectedMonthNumber { get; set; }

        public DataGridConfig<ProfitCenterCostItem>? CostGrid { get; set; }

        public List<PeriodMonth> PeriodMonths { get; set; } = new();
    }
    public class PeriodMonth
    {
        public string PeriodName { get; set; } = null!;
        public string? EndPeriod { get; set; }
    }
}
