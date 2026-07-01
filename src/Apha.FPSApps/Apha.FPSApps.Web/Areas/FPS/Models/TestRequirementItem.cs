/*
 * TRANSFORMENGINE MIGRATION — TestRequirementItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DataGrid item class derived from JS DataGridComponent columns in testList_VLA.js:
 *       initializeStage2TabsGrids() stage2TestRequirementsGrid columns array:
 *         project (120px), noTests (90px), agPrice (90px, £)
 *   - Property names map to TestRequirementDto fields via ForMember AutoMapper projections
 *     (JS field names differ from DTO names):
 *       JS "project"  → DTO Buyer       (project buyer code for the test requirement)
 *       JS "noTests"  → DTO NoRequired  (number of tests required)
 *       JS "agPrice"  → DTO UnitPrice   (agreed/agency price per test)
 *   - Composite PK: TestCode + Buyer (FpsYear is implicit context from parent row)
 *   - TestCode and FpsYear are hidden — context from parent TestListVla selected row
 *   - agPrice rendered with formatMoney in JS → GridColumnType.GbpValue
 *   - Tab modal fields (from JS createCrudGrid config fields): project (Buyer)*, noTests*, agPrice*
 *
 * PRESERVED:
 *   - All 3 JS visible columns plus hidden context fields (TestCode, FpsYear)
 *   - JS column width values used verbatim for GridColumn Width
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper must add explicit ForMember projections for
 *     TestRequirementItem ↔ TestRequirementDto because field names differ:
 *       Buyer ↔ Buyer (same), NoRequired ↔ NoRequired (same), UnitPrice ↔ UnitPrice (same)
 *     Convention-based ReverseMap should work; verify in AutoMapper profile.
 *   - TRANSFORMENGINE TODO: TestCode FK (fps.testorproduct) — must match parent row itemCode.
 *   - TRANSFORMENGINE TODO: Additional TestRequirementDto fields (Active, IsDefraProject,
 *     ProjectBuyerCode, TestBuyerCode, RecUnitPrice) not shown in grid — hidden if needed for
 *     full service-layer round-trip.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Grid row item for Test Requirements tab — JS stage2TestRequirementsGrid
    public class TestRequirementItem
    {
        // TRANSFORMENGINE: JS columns[0] { field: "project", header: "Project", width: 120 }
        // Maps to DTO Buyer (project buyer code) — part of composite PK
        [Required(ErrorMessage = "Project (Buyer) is required.")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        // TRANSFORMENGINE: JS columns[1] { field: "noTests", header: "No Tests", width: 90 }
        // Maps to DTO NoRequired (double? in DTO — using decimal? for grid rendering)
        [Required(ErrorMessage = "No Tests is required.")]
        [Display(Name = "No Tests")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? NoRequired { get; set; }

        // TRANSFORMENGINE: JS columns[2] { field: "agPrice", header: "AgPrice", width: 90, render: formatMoney }
        // Maps to DTO UnitPrice (agreed/agency price per test)
        [Required(ErrorMessage = "AgPrice is required.")]
        [Display(Name = "AgPrice")]
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? UnitPrice { get; set; }

        // TRANSFORMENGINE: Composite PK context — hidden, populated from parent TestListVla row selection
        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: Additional DTO fields not in JS grid columns — hidden for service round-trip
        [GridColumn(IsVisible = false)]
        public string? ProjectBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public string? TestBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public short? Active { get; set; }

        [GridColumn(IsVisible = false)]
        public decimal? RecUnitPrice { get; set; }
    }
}
