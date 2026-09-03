using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class AnimalPlanItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int IndCounter { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string JobCode { get; set; } = null!;

        [Required(ErrorMessage = "Animal type is required")]
        [Display(Name = "Animal Type")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string AnimalType { get; set; } = null!;
        
        [Display(Name = "Days")]
        [NonFinancialRange]
        [GridColumn(Width = 80, Type = GridColumnType.DecimalNumber)]
        public double NumberOfDays { get; set; }

        [Display(Name = "No. Required")]
        [NonFinancialRange]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber)]
        public double NumberOfAnimals { get; set; }

        [Display(Name = "Daily Rate")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Animal Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? AnimalCost { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> AnimalTypeList { get; set; } = new List<SelectListItem>();
    }
}
