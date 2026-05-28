using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models;
public class SummarisedWgTimeViewModel
{
    public DataGridConfig<SummarisedWgTimePivotRow> Grid { get; set; } = new DataGridConfig<SummarisedWgTimePivotRow>();
    public SummarisedWgTimeSummary Summary { get; set; } = new();
    public string? SelectedWorkgroup { get; set; }
}