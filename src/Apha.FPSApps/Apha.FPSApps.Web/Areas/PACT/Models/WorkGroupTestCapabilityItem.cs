using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Model for WorkGroup-focused Test Capability grid items.
    /// Simplified view focused on WorkGroup context.
    /// </summary>
    public class WorkGroupTestCapabilityItem
    {
        [Display(Name = "Test")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Portfolio")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string PlanPortfolio { get; set; } = null!;


        [Display(Name = "SMS Code")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? SmsCode { get; set; }

        [Display(Name = "Predicted Outturn")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.Number, IsFilterable = false)]
        public double? PredOutturn { get; set; }

        [Display(Name = "Unit Cost")]
        [GridColumn(Order = 7, Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? UnitCost { get; set; }


        // Hidden fields for internal use
        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Notes { get; set; }

        [GridColumn(IsVisible = false)]
        public DateTime? LastModified { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ModifiedBy { get; set; }

    }
}
