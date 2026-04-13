using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgramProjectItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Width = 230, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "Programme")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Program { get; set; }

        [Display(Name = "Project Group")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? ProjectGroup { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Customer { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Contract { get; set; }

        [Display(Name = "Disease")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Disease { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Cost Inc")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? BudgetExt { get; set; }

        [Display(Name = "Trans Inc")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TransferIncome { get; set; }

        [Display(Name = "CW Debit")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PlanCaseWorkDebit { get; set; }
    }
}
