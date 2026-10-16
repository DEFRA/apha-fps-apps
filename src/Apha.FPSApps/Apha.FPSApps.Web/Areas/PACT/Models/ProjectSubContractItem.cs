using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectSubContractItem
    {
        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 381, Type = GridColumnType.Text)]
        public string? Project { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Month")]
        [GridColumn(Order = 2, Width = 99, Type = GridColumnType.Number, IsFilterable = true)]
        public double? Month { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Amount must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 3, Width = 117, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }

        [Display(Name = "AcctCode")]
        [GridColumn(Order = 4, Width = 295, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AcctCode { get; set; }

        [Display(Name = "Test")]
        [GridColumn(Order = 5, Width = 73, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestJob { get; set; }

        [Display(Name = "Counter")]
        [GridColumn(Order = 6, Width = 119, Type = GridColumnType.Number)]
        public int SubContCounter { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Order = 7, Type = GridColumnType.Text, IsVisible = false)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Supplier")]
        [GridColumn(Order = 8, Type = GridColumnType.Text, IsVisible = false)]
        public string? Supplier { get; set; }


        [Display(Name = "Supplier Number")]
        [GridColumn(Order = 9, Type = GridColumnType.Number, IsVisible = false)]
        public int? SupplierNumber { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 10, Type = GridColumnType.Text, IsVisible = false)]
        public string? Description { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Order = 11, Type = GridColumnType.Number, IsVisible = false)]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Animal Days")]
        [GridColumn(Order = 12, Type = GridColumnType.Number, IsVisible = false)]
        public int? AnimalDays { get; set; }
    }
}
