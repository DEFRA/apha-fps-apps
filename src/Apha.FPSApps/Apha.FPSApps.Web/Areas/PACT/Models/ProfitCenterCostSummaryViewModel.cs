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
        /// List of calendar periods for the period selector dropdown.
        /// </summary>
        public List<SelectListItem> Periods { get; set; } = [];

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
        public List<(string Period, string MonthNumber)> PeriodMonths { get; set; } = new();

    }
}
