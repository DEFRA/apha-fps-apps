using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BudgetBidExportRow
    {
        [Display(Name = "Work Group")]
        public string WorkGroupName { get; set; } = string.Empty;

        [Display(Name = "Account")]
        public string Account { get; set; } = string.Empty;

        [Display(Name = "Gen Bid (£)")]
        public decimal GenBid { get; set; }
    }
}
