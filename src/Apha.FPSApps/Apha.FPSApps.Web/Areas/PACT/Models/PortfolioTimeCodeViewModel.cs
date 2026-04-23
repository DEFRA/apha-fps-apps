using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PortfolioTimeCodeViewModel
    {
        [Display(Name = "WorkGrp")]
        [Required(ErrorMessage = "Work Group is required")]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Active")]
        [GridColumn(Order = 2, Width = 70, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Active { get; set; }

        [Display(Name = "Time Code")]
        [Required(ErrorMessage = "Time Code is required")]
        [StringLength(50)]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Project")]
        [Required(ErrorMessage = "Parent Project is required")]
        [StringLength(50)]
        [GridColumn(Order = 4, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Test Code")]
        [StringLength(50)]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "Portfolio")]
        [StringLength(50)]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Portfolio { get; set; }

        [GridColumn(IsVisible = false)]
        public string? OriginalWorkGroup { get; set; }
    }
}
