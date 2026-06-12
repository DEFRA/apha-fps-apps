// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — MaintWGStaffViewModel.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - New file — no legacy equivalent (MS Access form used bound controls, no ViewModel layer).
 *   - WGStaffGrid: DataGridConfig<WorkGroupEmployeeItem> — NEVER left as new(); built explicitly
 *     in MaintWGStaffController.Index() to prevent empty-grid / add-button-always-visible defect.
 *
 * PRESERVED:
 *   - N/A — new artefact.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: No page-level filter dropdowns present. The HTML prototype
 *     (source/ui/fps/frmMaintWGStaff.html) has no <select> elements outside the grid container.
 *     wgGrade appears as a grid column only. No SelectListItem properties are needed here.
 *   - TRANSFORMENGINE TODO: If a future requirement adds a wgGrade page-level filter, add
 *     public List<SelectListItem> WgGradeList { get; set; } = new(); here and wire it
 *     via PopulateDropdownsAsync in the controller.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: ViewModel for the MaintWGStaff page (frmMaintWGStaff → MaintWGStaff/Index.cshtml).
    // No page-level filter dropdowns — HTML prototype has no <select> outside the grid container.
    public class MaintWGStaffViewModel
    {
        // TRANSFORMENGINE: WGStaffGrid — primary DataGrid.
        // NEVER leave as new() — always built explicitly in MaintWGStaffController.Index().
        // Default = new() here satisfies compiler; runtime value always set by controller.
        public DataGridConfig<WorkGroupEmployeeItem> WGStaffGrid { get; set; } = new DataGridConfig<WorkGroupEmployeeItem>();
    }
}
