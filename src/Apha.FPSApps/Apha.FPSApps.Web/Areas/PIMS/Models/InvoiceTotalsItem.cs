using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class InvoiceTotalsItem
    {
        
        [Display(Name = "Total Planned Amount")]
        public double TotalPlannedAmount { get; set; }

        
        [Display(Name = "Total Amount Due")]
        public double TotalDueAmount { get; set; }

        
        [Display(Name = "Total Amount Invoiced")]
        public double TotalActualAmount { get; set; }
    }
}
