using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class EmployeeViewModel
    {
        [Display(Name = "SP Number")]
        [Required]
        [StringLength(50, ErrorMessage = "SP Number cannot exceed 50 characters")]
        [GridColumn(Width = 271, Type = GridColumnType.Text, IsFilterable = true)]
        public string? SPNumber { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters")]
        [GridColumn(Width = 335, Type = GridColumnType.Text, IsFilterable = true)]
        public string? LastName { get; set; }

        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters")]
        [GridColumn(Width = 266, Type = GridColumnType.Text, IsFilterable = true)]
        public string? FirstName { get; set; }

        [Display(Name = "Title")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [GridColumn(Width = 143, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Title { get; set; }
    }
}
