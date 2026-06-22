/*
 * TRANSFORMENGINE MIGRATION — CostCentreMaintenanceViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New ViewModel for Cost Centre Maintenance DataGrid form (frmMaintCostCentres)
 *   - CostCentreGrid: DataGridConfig<CostCentreItem> — built explicitly in controller Index()
 *   - ProfitCentreList: List<SelectListItem> — populated from ICostCentreService.GetAllCostCentresAsync()
 *     for the Profit Centre dropdown in the Add/Edit modal partial (_AddEditCostCentre.cshtml)
 *
 * PRESERVED:
 *   - No page-level filter dropdowns — HTML prototype contains no <select> elements outside
 *     the grid container; both modal selects (modal-cc-number, modal-cc-profit) are inside
 *     the CRUD modal and are not grid filter controls
 *   - DataGridConfig left as new() default here; always built explicitly in controller
 *     to avoid rendering an empty grid with wrong operations profile
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Phase 12 (_AddEditCostCentre.cshtml) must bind modal cost-centre-number
 *     select to a separate lookup — ICostCentreService does not currently expose a dedicated
 *     cost-centre-number lookup method; verify with backend team whether a CostCentreNo list
 *     endpoint is required or whether the number is free-typed.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Cost Centre Maintenance DataGrid form (frmMaintCostCentres).
    /// Grid config is built explicitly in <c>CostCentreMaintenanceController.Index()</c>.
    /// ProfitCentreList is populated from the workgroup lookup via
    /// <c>ICostCentreService.GetAllCostCentresAsync()</c> for use in the Add/Edit modal.
    /// </summary>
    public class CostCentreMaintenanceViewModel
    {
        // TRANSFORMENGINE: DataGridConfig built explicitly in CostCentreMaintenanceController.Index()
        // Leaving as new() would render an empty grid with default Add button regardless of JS-derived
        // operations profile (AllowAdd/Edit/Delete from costcenter_maintenance.js).
        public DataGridConfig<CostCentreItem> CostCentreGrid { get; set; } = new DataGridConfig<CostCentreItem>();

        // TRANSFORMENGINE: ProfitCentreList — distinct profit-centre values extracted from
        // CostCentreWorkgroupDto.ProfitCentre via GetAllCostCentresAsync(); used in the
        // _AddEditCostCentre modal partial for the Profit Centre dropdown (modal-cc-profit).
        public List<SelectListItem> ProfitCentreList { get; set; } = new List<SelectListItem>();
    }
}
