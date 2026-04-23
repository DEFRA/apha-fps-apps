using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PortfolioDetailModel
    {
        [Display(Name = "Parent Project")]
        public string? ParentProject { get; set; }

        [Display(Name = "Project Title")]
        [Required(ErrorMessage = "Project Title is required.")]
        [StringLength(255)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Finished")]
        public bool Finished { get; set; }

        [Display(Name = "Programme")]
        public string? Program { get; set; }

        [Display(Name = "Manager")]
        public string? ProjectManager { get; set; }

        [Display(Name = "Budget-cvt")]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Transfer Income")]
        public decimal? TransferIncome { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }
    }

    public class PortfolioMaintenanceViewModel
    {
        public PortfolioDetailModel CurrentPortfolio { get; set; } = new();

        public List<SelectListItem> PortfolioOptions { get; set; } = [];
        public List<SelectListItem> Programs { get; set; } = [];
        public List<SelectListItem> Managers { get; set; } = [];
        public List<SelectListItem> WorkGroups { get; set; } = [];
        public List<SelectListItem> TestorProducts { get; set; } = [];

        public DataGridConfig<ConstituentTestItem> ConstituentTestGrid { get; set; } = new();
        public DataGridConfig<PortfolioTimeCodeViewModel> TimeCodeGrid { get; set; } = new();
    }
}
