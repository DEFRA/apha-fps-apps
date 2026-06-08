using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AnimalCostActualItem
    {
        [Display(Name = "Month")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [Display(Name = "Acct Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? AcctCode { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly)]
        public string? Description { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Animal Days")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public int? AnimalDays { get; set; }

        [Display(Name = "Amount")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }
    }
}
