using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS
{
    public class YearMasterReq
    {
        [Required]
        [Range(2000, 2100, ErrorMessage = "FPS Year must be between 2000 and 2100")]
        public int FpsYear { get; set; }

        [Required(ErrorMessage = "FPS Year Code is required")]
        [StringLength(20, ErrorMessage = "FPS Year Code cannot exceed 20 characters")]
        public string FpsYearCode { get; set; } = null!;

        [Required(ErrorMessage = "Year Status is required")]
        [StringLength(10, ErrorMessage = "Year Status cannot exceed 10 characters")]
        public string YearStatus { get; set; } = null!;

        public string? Remarks { get; set; }

        public bool Active { get; set; }

        public string? CreatedBy { get; set; }
    }
}
