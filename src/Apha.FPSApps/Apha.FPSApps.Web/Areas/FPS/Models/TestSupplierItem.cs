using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid model for the FPS Test Supplier view.
    /// Read-only columns sourced from the FPS TestSupplier API (custom project-join view).
    /// CRUD operations use the PACT TestRequirement API.
    /// </summary>
    public class TestSupplierItem
    {
        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Buyer")]
        [Required(ErrorMessage = "Buyer is required.")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        [Display(Name = "Project Manager")]
        [GridColumn(Order = 3, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectManager { get; set; }

        [Display(Name = "No. Required")]
        [GridColumn(Order = 4, Width = 90, Type = GridColumnType.DecimalNumber)]
        public double? NoRequired { get; set; }

        [Display(Name = "Unit Price")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Test Cost")]
        [GridColumn(Order = 6, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? TestCost { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Order = 7, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        // ── Hidden — used by Add/Edit modal only ──────────────────────────────

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
