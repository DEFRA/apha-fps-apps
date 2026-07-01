/*
 * TRANSFORMENGINE MIGRATION — TestListVlaItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DataGrid item class derived from JS DataGridComponent columns in testList_VLA.js:
 *       initializeTopListGrid() stage2TestListGrid columns array:
 *         itemCode (95px), description (310px), shortDesc (150px),
 *         manager (90px), status (75px), unitPrice (120px, £), defaultPrice (130px, £)
 *   - Property names map exactly to TestListVlaDto fields for ReverseMap AutoMapper convention
 *   - ItemCode is the visible PK column — kept visible (present in JS columns[]) and also KeyProperty
 *   - FpsYear is hidden — not in JS columns[] (composite PK context, sourced from page year selector)
 *   - unitPrice / defaultPrice rendered with formatMoney in JS → GridColumnType.GbpValue
 *   - Add/Edit modal fields (from HTML vlaTestListModal): itemCode*, description*, shortDesc,
 *     manager, status, unitPrice*, defaultPrice* — required fields match HTML aria-required="true"
 *
 * PRESERVED:
 *   - All 7 JS column fields plus hidden FpsYear composite PK context
 *   - JS column width values used verbatim for GridColumn Width
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: owner, chargeMethod, priceAhvg fields exist in TestListVlaDto but
 *     are not present in JS grid columns — they are not displayed in the grid. If the Add/Edit
 *     modal needs them, add them as hidden grid columns.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Grid row item for Test List for VLA — JS DataGridComponent stage2TestListGrid
    public class TestListVlaItem
    {
        // TRANSFORMENGINE: Composite PK part 1 — visible in JS columns[] → keep visible, also KeyProperty
        [Required(ErrorMessage = "Item Code is required.")]
        [Display(Name = "ItemCode")]
        [GridColumn(Order = 1, Width = 95, Type = GridColumnType.Text, IsFilterable = true)]
        public string ItemCode { get; set; } = null!;

        // TRANSFORMENGINE: JS columns[1] { field: "description", header: "Description", width: 310 }
        // HTML modal vla-form-description — aria-required="true" → [Required]
        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 310, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        // TRANSFORMENGINE: JS columns[2] { field: "shortDesc", header: "Short Desc", width: 150 }
        // Maps to DTO ShortDescription — not required in modal
        [Display(Name = "Short Desc")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ShortDescription { get; set; }

        // TRANSFORMENGINE: JS columns[3] { field: "manager", header: "Manager", width: 90 }
        // Maps to DTO TestManager — not required in modal
        [Display(Name = "Manager")]
        [GridColumn(Order = 4, Width = 90, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestManager { get; set; }

        // TRANSFORMENGINE: JS columns[4] { field: "status", header: "Status", width: 75 }
        // Maps to DTO JobStatus — not required in modal
        [Display(Name = "Status")]
        [GridColumn(Order = 5, Width = 75, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobStatus { get; set; }

        // TRANSFORMENGINE: JS columns[5] { field: "unitPrice", header: "UnitPrice(Std)", width: 120, render: formatMoney }
        // Maps to DTO UnitPriceVla — aria-required="true" in modal → [Required]
        [Required(ErrorMessage = "Unit Price (Std) is required.")]
        [Display(Name = "UnitPrice(Std)")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? UnitPriceVla { get; set; }

        // TRANSFORMENGINE: JS columns[6] { field: "defaultPrice", header: "DefaultUnitPrice", width: 130, render: formatMoney }
        // Maps to DTO DefraUnitPrice (NOT NULL DEFAULT 0 in DDL) — aria-required="true" in modal
        [Required(ErrorMessage = "Default Unit Price is required.")]
        [Display(Name = "DefaultUnitPrice")]
        [GridColumn(Order = 7, Width = 130, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal DefraUnitPrice { get; set; }

        // TRANSFORMENGINE: Composite PK part 2 — hidden (not in JS columns[]) — carries year context
        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: DTO fields not shown in JS grid — hidden, available for modal/service use
        [GridColumn(IsVisible = false)]
        public decimal? PriceAhvg { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Owner { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ChargeMethod { get; set; }
    }
}
