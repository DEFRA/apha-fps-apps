using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestOrProductViewModel
    {
        [Display(Name = "Item Code")]
        [Required(ErrorMessage = "Item Code is required")]
        [StringLength(20)]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text,IsFilterable =true)]
        public string ItemCode { get; set; } = null!;
       
        [Display(Name = "Item Description")]
        [StringLength(200)]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        [Display(Name = "Short Description")]
        [StringLength(18)]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text)]
        public string? ShortDescription { get; set; }


        [Display(Name = "Owner")]
        [Required(ErrorMessage = "Owner is required")]
        [StringLength(2)]
        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Owner { get; set; }

        [Display(Name = "Test Manager")]
        [StringLength(50)]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestManager { get; set; }
       
        [Display(Name = "Unit Price")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.Currency)]
        public decimal? UnitPriceVla { get; set; }

        [Display(Name = "DEFRA Unit Price")]
        [Required(ErrorMessage = "DEFRA Unit Price is required")]
        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.Currency)]
        public decimal DefraUnitPrice { get; set; }
        public int FpsYear { get; set; }
    }
}

