using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BudgetResourceLevelViewModel
    {
        public string? SelectedProfitCentre { get; set; }

        public List<SelectListItem> ProfitCentreList { get; set; } = new();

        public string? SelectedWorkgroup { get; set; }

        public string? SelectedAccount { get; set; }

        public decimal TotalBid { get; set; }

        public decimal TotalPurchases { get; set; }

        public DataGridConfig<WorkGroupItem> WorkGroupGrid { get; set; } = new DataGridConfig<WorkGroupItem>();

        public DataGridConfig<BudgetResourceCentreLevelItem> BudgetBidsGrid { get; set; } = new DataGridConfig<BudgetResourceCentreLevelItem>();

        public DataGridConfig<PurchaseItem> PurchasesGrid { get; set; } = new DataGridConfig<PurchaseItem>();
    }
}
