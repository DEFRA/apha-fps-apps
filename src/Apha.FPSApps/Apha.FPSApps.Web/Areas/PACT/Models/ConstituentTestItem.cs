using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ConstituentTestItem
    {
        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Item Description")]
        [GridColumn(Order = 2, Width = 300, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        [Display(Name = "Work Group")]
        [Required(ErrorMessage = "Work Group is required.")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Plan Portfolio")]
        [Required(ErrorMessage = "Plan Portfolio is required.")]
        [GridColumn(IsVisible = false)]
        public string PlanPortfolio { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public decimal? UnitCost { get; set; }

        [GridColumn(IsVisible = false)]
        public double? PredOutturn { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Sop { get; set; }

        [GridColumn(IsVisible = false)]
        public string? SmsCode { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
