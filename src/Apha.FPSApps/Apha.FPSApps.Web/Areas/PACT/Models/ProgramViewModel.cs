using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProgramViewModel
    {
        [Required(ErrorMessage = "Program number is required")]
        [StringLength(10, ErrorMessage = "Program number cannot exceed 10 characters")]
        [Display(Name = "Program No")]
        public string ProgramNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Program name is required")]
        [StringLength(80, ErrorMessage = "Program name cannot exceed 80 characters")]
        [Display(Name = "Program Name")]
        public string ProgramName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Sector name cannot exceed 100 characters")]
        [Display(Name = "Sector Name")]
        public string? SectorName { get; set; }

        [StringLength(50, ErrorMessage = "Customer cannot exceed 100 characters")]
        [Display(Name = "Customer")]
        public string? Customer { get; set; }

        [StringLength(50, ErrorMessage = "Leader cannot exceed 50 characters")]
        [Display(Name = "Leader")]
        public string? Manager { get; set; }

        [StringLength(7, ErrorMessage = "MINIM cannot exceed 20 characters")]
        [Display(Name = "MINIM")]
        public string? Minim { get; set; }

        [StringLength(15, ErrorMessage = "Directorate cannot exceed 50 characters")]
        [Display(Name = "Directorate")]
        public string? Directorate { get; set; }
    }
}
