using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectProfileViewModel
    {
        public string ParentProject { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Manager { get; set; }
        public string? ProjectStatus { get; set; }
        public decimal? BudgetCvl { get; set; }
        public decimal? BudgetExt { get; set; }
        public DataGridConfig<ProjectMonthItem> CostProfileGrid { get; set; } = new DataGridConfig<ProjectMonthItem>();
        public decimal TotalCostProfile { get; set; }
    }
}