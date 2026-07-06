/*
 * TRANSFORMENGINE MIGRATION — MaintenanceViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New multi-tab ViewModel for the Maintenance page (frmMaintainance)
 *   - Six functional tabs:
 *       1. Reports Tab     — ReportsGrid (ReportItem) + ReportGroupsGrid (ReportGroupItem)
 *       2. Programme Tab   — RadTrackProgsGrid (RadTrackProgItem)
 *       3. Manager Tab     — ProjectManagersGrid (ProjectManagerItem),
 *                            ProgramManagerLinksGrid (ProgramManagerLinkItem),
 *                            ProfitCentreManagerLinksGrid (ProfitCentreManagerLinkItem)
 *       4. Time Tab        — WorkingHoursSettingItem + WorkingDaysSettingItem (direct form binding)
 *       5. Admin Maint Tab — AccessUsersGrid (AccessUserItem) + AccessUserLevelsGrid (AccessUserLevelItem)
 *       6. Other Tab       — FrequenciesGrid (FrequencyItem) + ReviewItemsGrid (ReviewItemItem)
 *   - No page-level filter dropdowns: HTML has no <select> outside any grid container
 *     for standalone filtering; all selects are inside modals
 *
 * PRESERVED:
 *   - Tab structure matches frmMaintainance.html exactly
 *   - All grid DataGridConfig properties built explicitly in controller — never left as new()
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SelectedManagerName used for Manager sub-grid context; confirm
 *     this is the correct way to pass selected-row context to sub-grids in the MVC layer
 *   - TRANSFORMENGINE TODO: WorkingHoursSettingItem and WorkingDaysSettingItem are resolved
 *     by their known setting keys — verify key names match backend tbl_settings data
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Multi-tab ViewModel for frmMaintainance page — 6 tabs, 10 DataGridConfig instances
    public class MaintenanceViewModel
    {
        // ── Reports Tab ──────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: Main reports grid (gridContainer_maintenanceTable in HTML)
        public DataGridConfig<ReportItem> ReportsGrid { get; set; } = new();

        // TRANSFORMENGINE: Report groups sub-grid (gridContainer_reportGroupsTable in HTML)
        public DataGridConfig<ReportGroupItem> ReportGroupsGrid { get; set; } = new();

        // TRANSFORMENGINE: Currently selected report ID (for report-groups sub-grid context)
        public int? SelectedReportId { get; set; }

        // ── Programme Tab ────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: PIMS Programmes grid (gridContainer_pimsProgTable in HTML)
        public DataGridConfig<RadTrackProgItem> RadTrackProgsGrid { get; set; } = new();

        // ── Manager Tab ──────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: Manager grid (gridContainer_mgrTable in HTML)
        public DataGridConfig<ProjectManagerItem> ProjectManagersGrid { get; set; } = new();

        // TRANSFORMENGINE: Program assignments sub-grid (gridContainer_mgrProgramTable in HTML)
        public DataGridConfig<ProgramManagerLinkItem> ProgramManagerLinksGrid { get; set; } = new();

        // TRANSFORMENGINE: Resource Centre assignments sub-grid (gridContainer_mgrResourceTable in HTML)
        public DataGridConfig<ProfitCentreManagerLinkItem> ProfitCentreManagerLinksGrid { get; set; } = new();

        // TRANSFORMENGINE: Currently selected manager name (for sub-grid context)
        public string? SelectedManagerName { get; set; }

        // ── Time Tab ─────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: Working Hours setting (timeWorkingHours input in HTML, step=0.1, value=7.2)
        public SettingItem? WorkingHoursSettingItem { get; set; }

        // TRANSFORMENGINE: Working Days setting (timeWorkingDays input in HTML, step=0.5, value=220.5)
        public SettingItem? WorkingDaysSettingItem { get; set; }

        // ── Admin Maintenance Tab ────────────────────────────────────────────────────

        // TRANSFORMENGINE: Users grid (gridContainer_adminUsersTable in HTML)
        // admin.js columns: ntlogin (170), username (240), actions (120)
        public DataGridConfig<AccessUserItem> AccessUsersGrid { get; set; } = new();

        // TRANSFORMENGINE: User Access grid (gridContainer_adminAccessTable in HTML)
        // admin.js columns: user (220), accessLevel (180), actions (120)
        public DataGridConfig<AccessUserLevelItem> AccessUserLevelsGrid { get; set; } = new();

        // ── Other Tab ────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: Frequency lookup grid (otherListTable / otherValuesTable — Frequency section)
        public DataGridConfig<FrequencyItem> FrequenciesGrid { get; set; } = new();

        // TRANSFORMENGINE: ReviewItem lookup grid (otherListTable / otherValuesTable — ReviewItem section)
        public DataGridConfig<ReviewItemItem> ReviewItemsGrid { get; set; } = new();
    }
}
