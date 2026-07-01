/*
 * TRANSFORMENGINE MIGRATION — TestListVlaViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New MVC ViewModel created for the Test List for VLA page
 *   - Hosts five DataGridConfig instances matching the five JS DataGridComponent instances
 *     in testList_VLA.js: main test list grid + four tab grids
 *   - No page-level filter dropdowns — HTML prototype has no explicit <select> outside the grid
 *   - FpsYear bound from IFpsYearContext (page-level year selector, not a standalone dropdown)
 *   - Property names mirror TestListVlaDto fields exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - Separation of concerns: CRUD grid (TestListGrid) vs sub-resource tab grids
 *     (TestRequirementsGrid, ComponentChargesGeneralGrid, ComponentChargesProjectGrid, SuppliersGrid)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsYear populated from IFpsYearContext.Year in the controller —
 *     verify that the year selector widget on the page writes back to IFpsYearContext correctly.
 *   - TRANSFORMENGINE TODO: Summary computed fields (TotalRequired, ComponentTotal, VlaUnitPrice)
 *     shown in HTML prototype are client-side computed — no ViewModel properties needed unless
 *     server-side pre-population is required.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: ViewModel for frmTestList / testList_VLA.js — one grid per JS DataGridComponent instance
    public class TestListVlaViewModel
    {
        // TRANSFORMENGINE: FpsYear — required business context for all API calls; sourced from IFpsYearContext
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: Main test list grid — JS gridId: "stage2TestListGrid" / "testListForVlaGrid"
        // AllowAdd/Edit/Delete = true (CRUD modals present in HTML prototype: vlaTestListModal, vlaDeleteModal)
        public DataGridConfig<TestListVlaItem> TestListGrid { get; set; } = new();

        // TRANSFORMENGINE: Test Requirements tab grid — JS gridId: "stage2TestRequirementsGrid"
        // CRUD sub-resource: TestRequirements per selected TestListVla item
        public DataGridConfig<TestRequirementItem> TestRequirementsGrid { get; set; } = new();

        // TRANSFORMENGINE: Component Charges (general/RC cost) tab grid — JS gridId: "stage2ComponentGeneralGrid"
        // CRUD sub-resource: TestRCCost per selected TestListVla item
        public DataGridConfig<TestRCCostItem> ComponentChargesGeneralGrid { get; set; } = new();

        // TRANSFORMENGINE: Component Charges (project-specific) tab grid — JS gridId: "stage2ComponentProjectGrid"
        // CRUD sub-resource: TestRequirementRCCost per selected TestListVla item
        public DataGridConfig<TestRequirementRCCostItem> ComponentChargesProjectGrid { get; set; } = new();

        // TRANSFORMENGINE: Suppliers/WorkGroups tab grid — JS gridId: "stage2SuppliersGrid"
        // CRUD sub-resource: TestCapability per selected TestListVla item
        public DataGridConfig<TestCapabilityItem> SuppliersGrid { get; set; } = new();
    }
}
