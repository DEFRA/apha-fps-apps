using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectInvoiceItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int InvoiceCounter { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 289, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProjectParent { get; set; } = null!;

        [Required(ErrorMessage = "Month is required")]        
        [GridColumn(Order = 2, Width = 75, Type = GridColumnType.Number, IsFilterable = true)]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Amount is required")]        
        [GridColumn(Order = 3, Width = 89, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }
        
        [GridColumn(Order = 4, Width = 129, Type = GridColumnType.GbpValue)]
        public decimal? CostOfWork { get; set; }

        [Display(Name = "WIP")]
        [GridColumn(Order = 5, Width = 86, Type = GridColumnType.GbpValue)]
        public decimal? Wip { get; set; }
        
        [GridColumn(Order = 6, Width = 109, Type = GridColumnType.GbpValue)]
        public decimal? ProfitLoss { get; set; }
        
        [GridColumn(Order = 7, Width = 177, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Detail { get; set; }

        [Display(Name = "Invoice Counter")]
        [GridColumn(Order = 8, Width = 159, Type = GridColumnType.Number)]
        public int Counter { get; set; }
    }
}
