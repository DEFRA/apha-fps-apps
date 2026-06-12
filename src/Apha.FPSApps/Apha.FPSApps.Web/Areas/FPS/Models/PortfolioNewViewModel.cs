using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Portfolio Details edit form (frmPortfolioNew).
    /// Covers the subset of tlkpProject fields shown on that form.
    /// </summary>
    public class PortfolioNewViewModel
    {
        [Display(Name = "Portfolio Code")]
        public string ParentProject { get; set; } = null!;

        [Required]
        [Display(Name = "Description")]
        public string ProjectTitle { get; set; } = null!;

        [Required]
        [Display(Name = "Programme")]
        public string Program { get; set; } = null!;

        [Display(Name = "Manager")]
        public string? Manager { get; set; }

        [Required]
        [Display(Name = "Disease")]
        public string Disease { get; set; } = null!;

        [Required]
        [Display(Name = "Status")]
        public string ProjectStatus { get; set; } = null!;

        [Required]
        [Display(Name = "Predicted Transfer Income")]
        public decimal TransferIncome { get; set; }

        [Required]
        [Display(Name = "Customer Income")]
        public decimal CustIncome { get; set; }

        [Required]
        [Display(Name = "Target Profit")]
        public decimal? Profit { get; set; }

        [Required]
        [Display(Name = "Contract No")]
        public string Contract { get; set; } = null!;

        [Required]
        [Display(Name = "Customer")]
        public string Customer { get; set; } = null!;

        // ── Dropdown lists ────────────────────────────────────────────────────

        public List<SelectListItem> ProgramList { get; set; } = new();
        public List<SelectListItem> ManagerList { get; set; } = new();
        public List<SelectListItem> DiseaseList { get; set; } = new();
        public List<SelectListItem> ProjectStatusList { get; set; } = new();
        public List<SelectListItem> ContractList { get; set; } = new();
        public List<SelectListItem> CustomerList { get; set; } = new();
    }
}
