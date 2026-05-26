using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    
    public class AutomaticInvoiceItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int InvoiceCounter { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 289, Type = GridColumnType.Text, IsFilterable = false)]
        public string ProjectParent { get; set; } = null!;

        [Required(ErrorMessage = "Month is required")]        
        [GridColumn(Order = 2, Width = 75, Type = GridColumnType.Number, IsFilterable = false)]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Amount is required")]        
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }
    }
}
