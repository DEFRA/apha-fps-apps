using System;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class YearlyFinancialDataExportItem
    {
        public string? Project { get; set; }

        public int Year { get; set; }

        [Display(Name = "PP/Acc")]
        public decimal? BfBudget { get; set; }

        [Display(Name = "Customer Income")]
        public decimal? PyBudget { get; set; }

        [Display(Name = "VLA Budget")]
        public decimal? VlaBudget { get; set; }

        [Display(Name = "Actual Exp")]
        public decimal? ActualExpenditure { get; set; }

        public decimal? Seedcorn { get; set; }

        [Display(Name = "Man Hours")]
        public double? ManHours { get; set; }

        [Display(Name = "Pay Costs")]
        public decimal? PayCosts { get; set; }

        [Display(Name = "Non-Pay & OH")]
        public decimal? NonPayOhCosts { get; set; }

        [Display(Name = "Test Costs")]
        public decimal? TestCosts { get; set; }

        [Display(Name = "Project Specific")]
        public decimal? NonAnimalCosts { get; set; }

        [Display(Name = "Animal Costs")]
        public decimal? AnimalCosts { get; set; }

        [Display(Name = "Exc/Adj")]
        public decimal? Adjustment { get; set; }

        [Display(Name = "Adj Comment")]
        public string? AdjustmentComment { get; set; }
        
    }
}
