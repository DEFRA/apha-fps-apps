/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DataGrid item class derived from JS DataGridComponent columns in testList_VLA.js:
 *       initializeStage2TabsGrids() stage2ComponentProjectGrid columns array:
 *         profitCentre (140px), project (120px), price (90px, £)
 *   - Property names map exactly to TestRequirementRCCostDto fields for AutoMapper convention
 *   - Composite PK: TestCode + Buyer + ProfitCentre + FpsYear
 *   - JS "project" field maps to DTO Buyer (project buyer code)
 *   - ProfitCentre and Buyer are visible grid columns (also part of composite PK)
 *   - TestCode and FpsYear are hidden — context from parent TestListVla selected row
 *   - price rendered with formatMoney in JS → GridColumnType.GbpValue
 *   - Tab modal fields (from JS createCrudGrid config fields): profitCentre*, project (Buyer)*, price*
 *
 * PRESERVED:
 *   - All 3 JS visible columns plus hidden composite PK context fields (TestCode, FpsYear)
 *   - JS column width values used verbatim for GridColumn Width
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Buyer FK (fps.tlkptestreqmt(testcode, buyer, fpsyear)) — validated
 *     at service layer. JS "project" label maps to the Buyer column semantically.
 *   - TRANSFORMENGINE TODO: ProfitCentre FK (fps.tbltestrccost) — a matching TestRCCost row must
 *     exist before insert; validated at service layer.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Grid row item for Component Charges (project-specific) tab — JS stage2ComponentProjectGrid
    public class TestRequirementRCCostItem
    {
        // TRANSFORMENGINE: JS columns[0] { field: "profitCentre", header: "ProfitCentre", width: 140 }
        // Part of composite PK — visible, editable
        [Required(ErrorMessage = "Profit Centre is required.")]
        [Display(Name = "ProfitCentre")]
        [GridColumn(Order = 1, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: JS columns[1] { field: "project", header: "Project", width: 120 }
        // Maps to DTO Buyer — "project" in the JS is the buyer/project code
        [Required(ErrorMessage = "Buyer (Project) is required.")]
        [Display(Name = "Project")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        // TRANSFORMENGINE: JS columns[2] { field: "price", header: "Price", width: 90, render: formatMoney }
        // Maps to DTO Price (NOT NULL — no DEFAULT in DDL)
        [Required(ErrorMessage = "Price is required.")]
        [Display(Name = "Price")]
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal Price { get; set; }

        // TRANSFORMENGINE: Composite PK context — hidden, populated from parent TestListVla row selection
        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
