using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class InvoiceImportFailedItem
    {
        [Display(Name = "Project Parent")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text)]
        public string? ProjectParent { get; set; }

        [Display(Name = "Month")]
        [RegularExpression(@"^(?:[1-9]|1[0-2])$", ErrorMessage = "Month must be between 1 and 12.")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.Text)]
        public string? Month { get; set; }

        [Display(Name = "Amount")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Amount must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.RoundTwoDecimal)]
        public string? Amount { get; set; }

        [Display(Name = "Cost Of Work")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Cost Of Work must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.RoundTwoDecimal)]
        public string? CostOfWork { get; set; }

        [Display(Name = "WIP")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "WIP must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.RoundTwoDecimal)]
        public string? Wip { get; set; }

        [Display(Name = "ProfitLoss")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Profit/Loss must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.RoundTwoDecimal)]
        public string? ProfitLoss { get; set; }

        [Display(Name = "Detail")]
        [GridColumn(Order = 7, Width = 150, Type = GridColumnType.Text)]
        public string? Detail { get; set; }

        [Display(Name = "Type")]
        [GridColumn(Order = 8, Width = 80, Type = GridColumnType.Text)]
        public string? Type { get; set; }

        [Display(Name = "Validation Failure")]
        [GridColumn(Order = 9, Width = 250, Type = GridColumnType.Text, CssClass = "grid-column-truncate-tooltip")]
        public string? ValidationFailure { get; set; }

        [GridColumn(IsVisible = false)]
        [ExcelHiddenColumn]
        public int Id { get; set; }
    }
}
