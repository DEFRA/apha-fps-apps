/*
 * TRANSFORMENGINE MIGRATION — TestRCCostItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DataGrid item class derived from JS DataGridComponent columns in testList_VLA.js:
 *       initializeStage2TabsGrids() stage2ComponentGeneralGrid columns array:
 *         profitCentre (140px), price (90px, £)
 *   - Property names map exactly to TestRCCostDto fields for ReverseMap AutoMapper convention
 *   - Composite PK: TestCode + ProfitCentre + FpsYear
 *   - ProfitCentre is a visible grid column (also part of composite PK)
 *   - TestCode and FpsYear are hidden — context from parent TestListVla selected row
 *   - price rendered with formatMoney in JS → GridColumnType.GbpValue
 *   - Tab modal fields (from JS createCrudGrid config fields): profitCentre*, price*
 *
 * PRESERVED:
 *   - All 2 JS visible columns plus hidden composite PK context fields (TestCode, FpsYear)
 *   - JS column width values used verbatim for GridColumn Width
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ProfitCentre FK (fps.tblkpprofitcentre) — validated at service layer.
 *   - TRANSFORMENGINE TODO: TestCode FK (fps.testorproduct) — must match parent selected row item code.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Grid row item for Component Charges (general) tab — JS stage2ComponentGeneralGrid
    public class TestRCCostItem
    {
        // TRANSFORMENGINE: JS columns[0] { field: "profitCentre", header: "ProfitCentre", width: 140 }
        // Part of composite PK — visible, editable
        [Required(ErrorMessage = "Profit Centre is required.")]
        [Display(Name = "ProfitCentre")]
        [GridColumn(Order = 1, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: JS columns[1] { field: "price", header: "Price", width: 90, render: formatMoney }
        // Maps to DTO Price (NOT NULL DEFAULT 0)
        [Required(ErrorMessage = "Price is required.")]
        [Display(Name = "Price")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal Price { get; set; }

        // TRANSFORMENGINE: Composite PK context — hidden, populated from parent TestListVla row selection
        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
