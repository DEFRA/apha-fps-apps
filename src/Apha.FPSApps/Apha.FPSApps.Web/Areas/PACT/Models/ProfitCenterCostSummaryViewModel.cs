using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for the Profit Center Cost Summary page.
    /// </summary>
    public class ProfitCenterCostSummaryViewModel
    {
        /// <summary>
        /// The currently selected month number, if any.
        /// </summary>
        public short? SelectedMonthNumber { get; set; }

        /// <summary>
        /// Data grid configuration for profit center costs.
        /// </summary>
        public DataGridConfig<ProfitCenterCostItem>? CostGrid { get; set; }

        /// <summary>
        /// List of release periods with Period name and MonthNumber.
        /// </summary>
        public List<PeriodMonth> PeriodMonths { get; set; } = new();
    }
    public class PeriodMonth
    {
        public string Period { get; set; } = null!;
        public string? MonthNumber { get; set; }
    }
}
