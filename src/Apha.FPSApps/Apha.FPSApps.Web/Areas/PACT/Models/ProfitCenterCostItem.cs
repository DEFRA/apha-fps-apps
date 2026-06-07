using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Represents a profit center with its aggregated cost for grid display.
    /// </summary>
    public class ProfitCenterCostItem
    {
        [GridColumn(Order = 1)]
        public string ProfitCentre { get; set; } = string.Empty;

        [GridColumn(Order = 2, Type = GridColumnType.GbpValue)]
        public decimal Cost { get; set; }
    }
}
