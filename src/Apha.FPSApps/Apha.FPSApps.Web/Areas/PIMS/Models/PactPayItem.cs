using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class PactPayItem
    {
        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly)]
        public double Month { get; set; }

        [Display(Name = "Month Name")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly)]
        public string? MonthName { get; set; }

        [Display(Name = "Pay")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal Pay { get; set; }

        [Display(Name = "NonPay")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal NonPay { get; set; }

        [Display(Name = "Overheads")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal Overhead { get; set; }

        [Display(Name = "Total Staff Costs")]
        [GridColumn(Width = 140, Type = GridColumnType.GbpValue)]
        public decimal StaffCosts { get; set; }
    }
}
