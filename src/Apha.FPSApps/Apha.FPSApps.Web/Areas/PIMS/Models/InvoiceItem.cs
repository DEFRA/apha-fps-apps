using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class InvoiceItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int InvoiceCounter { get; set; }

       
        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Project { get; set; }

       
        [Display(Name = "Contract")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Contract { get; set; }

        
        [Display(Name = "Planned Amount")]
        [GridColumn(Order = 3, Width = 130, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public double? PlannedAmount { get; set; }

        
        [Display(Name = "Amount Due")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public double? DueAmount { get; set; }

       
        [Display(Name = "Date Due")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DueDate { get; set; }

        
        [Display(Name = "Amount Invoiced")]
        [GridColumn(Order = 6, Width = 130, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public double? ActualAmount { get; set; }

       
        [Display(Name = "Date JS Raised")]
        [GridColumn(Order = 7, Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateJobsheetRaised { get; set; }

        
        [Display(Name = "Invoice Ref")]
        [GridColumn(Order = 8, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string? InvoiceRef { get; set; }

        
        [Display(Name = "Paid?")]
        [GridColumn(Order = 9, Width = 70, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public short InvoicePaid { get; set; }

        
        [Display(Name = "Date Invoiced")]
        [GridColumn(Order = 10, Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateInvoiced { get; set; }
    }
}
