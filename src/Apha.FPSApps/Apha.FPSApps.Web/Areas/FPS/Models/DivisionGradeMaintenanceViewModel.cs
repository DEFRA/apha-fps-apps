using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class DivisionGradeViewModel
    {
        public DataGridConfig<DivisionGradeItem> DivisionGradeGrid { get; set; } = new DataGridConfig<DivisionGradeItem>();
        public List<SelectListItem> GradeCodeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DivisionList { get; set; } = new List<SelectListItem>();
        public int? SelectedYear { get; set; }
    }

    public class DivisionGradeItem
    {
        [Display(Name = "Division Grade")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsVisible = true, IsFilterable = true)]
        public string DivisionGradeCode { get; set; } = null!;

        [Required(ErrorMessage = "Grade code is required")]
        [Display(Name = "Grade Code")]
        [GridColumn(Width = 120, Type = GridColumnType.Dropdown, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Required(ErrorMessage = "Division is required")]
        [Display(Name = "Division")]
        [GridColumn(Width = 200, Type = GridColumnType.Dropdown, IsFilterable = true)]
        public string? Division { get; set; }

        [Display(Name = "Charge Rate")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ChargeRate { get; set; }

        [Display(Name = "Direct Rate")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? DirectRate { get; set; }

        [Display(Name = "Pay Rate")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PayRate { get; set; }

        [Display(Name = "NPR")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Npr { get; set; }

        [Display(Name = "OHR")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Ohr { get; set; }
    }
}
