using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractRmsFailedItem
    {
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Test Job")]
        [GridColumn(Order = 2, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestJob { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Order = 3, Width = 80, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Month { get; set; }

        [Display(Name = "Amount")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Amount { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Account Code")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AcctCode { get; set; }

        [Display(Name = "Supplier")]
        [GridColumn(Order = 7, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Supplier { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 8, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        [Display(Name = "Supplier Number")]
        [GridColumn(Order = 9, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? SupplierNumber { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? DailyRate { get; set; }

        [Display(Name = "Animal Days")]
        [GridColumn(Order = 11, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AnimalDays { get; set; }

        [Display(Name = "Validation Failure")]
        [GridColumn(Order = 12, Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ValidationFailure { get; set; }
    }
}
