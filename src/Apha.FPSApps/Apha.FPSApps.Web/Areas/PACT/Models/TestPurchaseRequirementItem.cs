using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestPurchaseRequirementItem
    {
        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "No Required")]
        [Range(0, double.MaxValue, ErrorMessage = "No Required must be 0 or greater.")]
        [GridColumn(Order = 2, Width = 110, Type = GridColumnType.DecimalNumber)]
        public double? NoRequired { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(0, 92233720368547758.07, ErrorMessage = "Unit Price must be between 0 and 92,233,720,368,547,758.07.")]
        [RegularExpression(@"^\d{1,17}(\.\d{0,2})?$", ErrorMessage = "Unit Price must have at most 2 decimal places.")]
        [GridColumn(Order = 3, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        // ── Hidden (not shown in grid, kept for form and mapping) ─────────

        [GridColumn(IsVisible = false)]
        public bool IsEdit { get; set; }

        [Required(ErrorMessage = "Buyer is required.")]
        [GridColumn(IsVisible = false)]
        public string Buyer { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public string? ProjectBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public string? TestBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public short? Active { get; set; }

        [GridColumn(IsVisible = false)]
        public short IsDefraProject { get; set; }

        [GridColumn(IsVisible = false)]
        public decimal? RecUnitPrice { get; set; }
    }
}
