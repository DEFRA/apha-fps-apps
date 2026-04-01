using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class CopyJobCodeRequest
    {
        public string SourceJobCode { get; set; } = null!;
        public bool CopyWorkGroup { get; set; }
        [Required]
        public string JobCodeId { get; set; } = null!;
        public string? JobCodeName { get; set; }
        public string? Type { get; set; }
        [Required]
        public string JobCodeWorkGroup { get; set; } = null!;
        [Required]
        public string ParentProject { get; set; } = null!;
    }
}
