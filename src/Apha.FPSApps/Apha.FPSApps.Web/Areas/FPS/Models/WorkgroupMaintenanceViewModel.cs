/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Phase 11 rewrite of Phase 10 stub — ViewModel class only; Item class moved to WorkgroupMaintenanceItem.cs
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP -> fps.workgroup)
 *   - No page-level filter dropdowns — HTML prototype has no <select> elements outside the modal container;
 *     ResourceCentre, CostCentre, and Owner selects are modal-only, served by AJAX [HttpGet] endpoints
 *   - DataGridConfig<WorkgroupMaintenanceItem> built explicitly in WorkgroupMaintenanceController.Index()
 *
 * PRESERVED:
 *   - Namespace consistent with all FPS area Models
 *   - DataGridConfig<WorkgroupMaintenanceItem> WorkgroupGrid property retained from Phase 10 stub
 *
 * DEFERRED: none — fully automated.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the WorkGroup Maintenance page (frmMaintWorkGroup2).
    /// Holds the DataGrid configuration for the workgroup list grid.
    /// No page-level filter dropdowns — all lookup data is served via AJAX from the Add/Edit modal.
    /// </summary>
    public class WorkgroupMaintenanceViewModel
    {
        // TRANSFORMENGINE: DataGridConfig built explicitly in WorkgroupMaintenanceController.Index() — never left as new()
        public DataGridConfig<WorkgroupMaintenanceItem> WorkgroupGrid { get; set; } = new DataGridConfig<WorkgroupMaintenanceItem>();
    }
}
