using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class YearlyFinancialDataItem
    {
        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Project { get; set; }

       
        [Required(ErrorMessage = "Year is required")]
        [Display(Name = "Year")]
        [GridColumn(Order = 1, Width = 60, Type = GridColumnType.Text, IsFilterable = false)]
        public short Year { get; set; }

       
        [Display(Name = "PP/Acc")]
        [CurrencyRange]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? BfBudget { get; set; }

      
        [Display(Name = "Customer Income")]
        [CurrencyRange]
        [GridColumn(Order = 3, Width = 115, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PyBudget { get; set; }

        
        [Display(Name = "VLA Budget")]
        [CurrencyRange]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? VlaBudget { get; set; }

        
        [Display(Name = "Actual Exp")]
        [CurrencyRange]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ActualExpenditure { get; set; }

        
        [Display(Name = "Seedcorn")]
        [CurrencyRange]
        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Seedcorn { get; set; }

        
        [Display(Name = "Man Hours")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? ManHours { get; set; }

        
        [Display(Name = "Pay Costs")]
        [CurrencyRange]
        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PayCosts { get; set; }

        
        [Display(Name = "Non-Pay & OH")]
        [CurrencyRange]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? NonPayOhCosts { get; set; }

        
        [Display(Name = "Test Costs")]
        [CurrencyRange]
        [GridColumn(Order = 10, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TestCosts { get; set; }

        
        [Display(Name = "Project Specific")]
        [CurrencyRange]
        [GridColumn(Order = 11, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? NonAnimalCosts { get; set; }

        
        [Display(Name = "Animal Costs")]
        [CurrencyRange]
        [GridColumn(Order = 12, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? AnimalCosts { get; set; }

        
        [Display(Name = "Exc/Adj")]
        [CurrencyRange]
        [GridColumn(Order = 13, Width = 80, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Adjustment { get; set; }

       
        [Display(Name = "Adj Comment")]
        [GridColumn(Order = 14, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? AdjustmentComment { get; set; }

        
        [Display(Name = "Total Costs")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public decimal? TotalCosts { get; set; }

        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ManDays { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ManYears { get; set; }

       
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ActualManYears { get; set; }

        
        [Display(Name = "Fixed")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short Locked { get; set; }

        
        [Display(Name = "Date Fixed")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public DateTime? DateCosted { get; set; }

        
        [Display(Name = "Fixed By")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? CostedBy { get; set; }

        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short ManHoursChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short PayCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short NonPayOhCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short TestCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short AnimalCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short NonAnimalCostsChanged { get; set; }
    }
}
