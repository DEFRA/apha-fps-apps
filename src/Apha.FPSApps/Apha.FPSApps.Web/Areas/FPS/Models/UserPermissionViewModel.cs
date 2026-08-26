using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class UserPermissionViewModel
    {
        [Display(Name = "User_ID")]
        [GridColumn(IsFilterable = false)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "SQL UserName is required")]
        [StringLength(50, ErrorMessage = "SQL UserName cannot exceed 50 characters")]
        [Display(Name = "SQL UserName")]
        [GridColumn(IsFilterable = true)]
        public string? Username { get; set; }

        [StringLength(250, ErrorMessage = "User cannot exceed 250 characters")]
        [Display(Name = "User")]
        [GridColumn(IsFilterable = true)]
        public string? Comments { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        [GridColumn(IsFilterable = true)]
        public string? UserEmail { get; set; }

        [StringLength(100, ErrorMessage = "DT2 UserName cannot exceed 100 characters")]
        [Display(Name = "DT2 UserName")]
        [GridColumn(IsFilterable = true)]
        public string? Dt2Username { get; set; }
    }
}
