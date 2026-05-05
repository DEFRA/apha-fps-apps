using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProgramProjectItem
    {
        [Display(Name = "Code")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Title")]
        [GridColumn(Width = 300, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "BudgCVL")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "BudgExt")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? BudgetExt { get; set; }

        [Display(Name = "ProjectStatus")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly)]
        public string? ProjectStatus { get; set; }
    }
}
