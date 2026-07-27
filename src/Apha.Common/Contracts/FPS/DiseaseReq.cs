using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS
{
    public class DiseaseReq
    {
        [Required(ErrorMessage = "Disease Name is required")]
        [StringLength(50, ErrorMessage = "Disease Name cannot exceed 50 characters")]
        public string DiseaseName { get; set; } = null!;
    }
}
