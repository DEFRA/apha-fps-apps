using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the Staff of WG Grade table (fsubWGStaff — edit + delete; no add).
    /// AllowAdd: false  AllowEdit: true  AllowDelete: true
    /// </summary>
    public class WorkGroupEmployeeItem
    {
        /// <summary>PACTid — hidden primary key used as KeyProperty.</summary>
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string PactId { get; set; } = null!;

        [Display(Name = "SP No")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string SpNumber { get; set; } = null!;

        [Display(Name = "Name")]
        [GridColumn(Width = 220, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Name { get; set; } = null!;

        [Display(Name = "HrsPaid")]
        [Required(ErrorMessage = "HrsPaid is required")]
        [NonFinancialRange]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsPaid { get; set; }

        [Display(Name = "Leave")]
        [Required(ErrorMessage = "Leave is required")]
        [NonFinancialRange]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double Leave { get; set; }

        [Display(Name = "SickSp")]
        [Required(ErrorMessage = "SickSp is required")]
        [NonFinancialRange]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double SickSpecial { get; set; }

        [Display(Name = "AtWork")]
        [NonFinancialRange]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsAvail { get; set; }

        [Display(Name = "Planable")]
        [GridColumn(Width = 100, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool MakeAvailable { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Width = 100, Type = GridColumnType.Text, IsFilterable = false, IsVisible = false)]
        public string PersonStatus { get; set; } = null!;

        [Display(Name = "Class")]
        [GridColumn(Width = 100, Type = GridColumnType.Text, IsFilterable = false, IsVisible = false)]
        public string? PersonClass { get; set; }
    }
}