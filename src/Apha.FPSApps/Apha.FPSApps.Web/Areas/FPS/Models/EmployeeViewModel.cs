using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class EmployeeViewModel
    {
        [Display(Name = "SP Number")]
        [StringLength(50, ErrorMessage = "SP Number cannot exceed 50 characters")]
        public string? SPNumber { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [Display(Name = "Title")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string? Title { get; set; }

        [Display(Name = "Filter Option")]
        public int? FilterOption { get; set; }
    }
}
