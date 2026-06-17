using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BudgetResourceCentreLevelItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string WorkGroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account is required")]
        [Display(Name = "Account")]
        [GridColumn(Width = 150, Type = GridColumnType.Dropdown, IsFilterable = true)]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gen Bid is required")]
        [Display(Name = "Gen Bid")]
        [Range(0, double.MaxValue, ErrorMessage = "Gen Bid must be a positive value")]
        [DataType(DataType.Currency)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal GenBid { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> AccountList { get; set; } = new List<SelectListItem>();

        [GridColumn(IsVisible = false)]
        public List<AccountCategoryDto> AccountFullList { get; set; } = new List<AccountCategoryDto>();
    }
}
