using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffJobItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int StaffID { get; set; }   

        [Required(ErrorMessage = "Staff name is required")]
        [Display(Name = "Staff Name")]
        [StringLength(200, ErrorMessage = "Staff name cannot exceed 200 characters")]
        [GridColumn(Width = 169, Type = GridColumnType.Text, IsFilterable = true)]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Rate is required")]
        [Display(Name = "Rate")]
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be a positive value")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 63, Type = GridColumnType.GbpValue)]
        public decimal ChargeRate { get; set; }
       
        [Required(ErrorMessage = "Hours are required")]
        [Display(Name = "Hrs")]
        [Range(0, int.MaxValue, ErrorMessage = "Hours must be a positive value")]
        [GridColumn(Width = 69, Type = GridColumnType.Number, IsFilterable = true)]
        public double PlannedHours { get; set; }
        
        [Required(ErrorMessage = "Days are required")]
        [Display(Name = "Days")]
        [Range(0, double.MaxValue, ErrorMessage = "Days must be a positive value")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 81, Type = GridColumnType.Number)]
        public decimal Days { get; set; }
        
        [Required(ErrorMessage = "Staff cost is required")]
        [Display(Name = "Staff Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Staff cost must be a positive value")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 104, Type = GridColumnType.GbpValue)]
        public decimal StaffCost { get; set; }
    }
}
