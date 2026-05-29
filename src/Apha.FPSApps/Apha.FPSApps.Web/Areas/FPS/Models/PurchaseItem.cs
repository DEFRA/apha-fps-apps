using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class PurchaseItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string WorkgroupName { get; set; } = string.Empty;

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item Description is required")]
        [Display(Name = "Item Description")]
        [StringLength(50, ErrorMessage = "Item Description cannot exceed 50 characters")]
        [GridColumn(Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string ItemDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value")]
        [DataType(DataType.Currency)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal Amount { get; set; }
    }
}
