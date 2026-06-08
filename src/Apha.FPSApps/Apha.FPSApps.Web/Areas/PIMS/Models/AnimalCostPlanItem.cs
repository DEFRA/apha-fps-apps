using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AnimalCostPlanItem
    {
        [Display(Name = "Animal Type")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly)]
        public string? AnimalType { get; set; }

        [Display(Name = "No. of Days")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public double? NumberOfDays { get; set; }

        [Display(Name = "No. of Animals")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly)]
        public double? NumberOfAnimals { get; set; }

        [Display(Name = "Rate")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? Rate { get; set; }

        [Display(Name = "Cost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public double? Cost { get; set; }
    }
}
