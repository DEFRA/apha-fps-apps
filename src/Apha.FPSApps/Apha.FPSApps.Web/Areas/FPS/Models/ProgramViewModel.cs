using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgramViewModel
    {
        [Required(ErrorMessage = "Program number is required")]
        [StringLength(50, ErrorMessage = "Program number cannot exceed 50 characters")]
        [Display(Name = "Program Number")]
        public required string ProgramNo { get; set; }

        
        [Required(ErrorMessage = "Program name is required")]
        [StringLength(255, ErrorMessage = "Program name cannot exceed 255 characters")]
        [Display(Name = "Program Name")]
        public required string ProgramName { get; set; }

        
        [StringLength(100, ErrorMessage = "Target cannot exceed 100 characters")]
        [Display(Name = "Target")]
        public string? Target { get; set; }

        
        [StringLength(100, ErrorMessage = "Manager name cannot exceed 100 characters")]
        [Display(Name = "Manager")]
        public string? Manager { get; set; }

       
        [Required(ErrorMessage = "Directorate is required")]
        [StringLength(50, ErrorMessage = "Directorate cannot exceed 50 characters")]       
        [Display(Name = "Directorate")]
        public required string Directorate { get; set; }

        public ProgramViewModel()
        {
            ProgramNo = string.Empty;
            ProgramName = string.Empty;
            Directorate = string.Empty;
        }
    }
}
