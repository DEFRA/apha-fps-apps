using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectProfileViewModel
    {
        public string ParentProject { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public decimal? BudgetCvl { get; set; }
        public List<SelectListItem> Projects { get; set; } = [];
    }
}