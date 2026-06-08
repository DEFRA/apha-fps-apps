using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AdditionalCostActualItem
    {
        [Display(Name = "Month")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [Display(Name = "AcctCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly)]
        public string? AcctCode { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly)]
        public string? Description { get; set; }

        [Display(Name = "Supplier")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly)]
        public string? Supplier { get; set; }

        [Display(Name = "Supplier No")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public int? SupplierNumber { get; set; }

        [Display(Name = "Amount")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }
    }
}
