/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New ViewModel for the read-only Department Income report page (frmDeptIncome)
 *   - Carries page-level filter state: SelectedProject, SelectedMonthFrom, SelectedMonthTo
 *   - ProjectList bound to projectSelect dropdown (populated from IProjectService.GetAllProjectsAsync)
 *   - PeriodList bound to the period-table-dropdown custom control
 *     (AccntsPeriod, MonthName, MonthNumber columns; populated from IDepartmentIncomeService.GetPeriodsAsync)
 *   - SnapshotGrid holds the Snapshot-tab DataGridConfig (periodName, finalSummariesRun, periodLocke)
 *   - DepartmentIncomeSnapshotItem is a companion class defined in this file — 3 columns from JS
 *     getPeriodColumns(): periodName (240px), finalSummariesRun (checkbox, 180px), periodLocke (checkbox, 150px)
 *   - The 5 query result item types (Time, Test, Animal, Additional, Totals) are in separate files
 *   - Query results for the 5 queries (Time/Tests/Animals/Additional/Totals) are rendered on demand
 *     via AJAX endpoints and shown in a modal grid — not held in the ViewModel
 *
 * PRESERVED:
 *   - All filter parameter names match backend IDepartmentIncomeService params exactly
 *     (project, monthFrom, monthTo — all optional)
 *   - VBA fnDeptIncomeMonthFrom default (1) and fnDeptIncomeMonthTo default (12 or monthFrom)
 *     applied in service layer (DepartmentIncomeService), not here
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether ProjectList should use IProjectService or a dedicated
 *     project lookup endpoint from IDepartmentIncomeService — currently uses IProjectService.GetAllProjectsAsync
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: ViewModel for the Department Income report page (frmDeptIncome)
    // Read-only report form — no CRUD DataGridConfigs; query results shown in modal on Run query click
    public class DepartmentIncomeViewModel
    {
        // TRANSFORMENGINE: Selected project filter — bound to projectSelect dropdown (empty = all projects)
        public string? SelectedProject { get; set; }

        // TRANSFORMENGINE: Selected period-from filter — bound to monthFromSelect hidden backing select
        // The visible control is periodFromDropdown (period-table-dropdown custom control)
        public int? SelectedMonthFrom { get; set; }

        // TRANSFORMENGINE: Selected period-to filter — bound to monthToSelect hidden backing select
        // The visible control is periodToDropdown (period-table-dropdown custom control)
        public int? SelectedMonthTo { get; set; }

        // TRANSFORMENGINE: Project dropdown items — named ProjectList ([FieldName]List where bound property = SelectedProject)
        // Populated from IProjectService.GetAllProjectsAsync() — uses ParentProject as both Value and Text
        public List<SelectListItem> ProjectList { get; set; } = new();

        // TRANSFORMENGINE: Period list for period-table-dropdown custom controls (from/to pickers)
        // Populated from IDepartmentIncomeService.GetPeriodsAsync()
        // AccntsPeriod = accounting period (1–12), MonthName = display name, MonthNumber = calendar month
        public List<PeriodItem> PeriodList { get; set; } = new();

        // TRANSFORMENGINE: Snapshot tab DataGrid — shows period summary rows for the selected project
        // JS DataGridComponent columns: periodName (240px), finalSummariesRun (checkbox, 180px), periodLocke (checkbox, 150px)
        // AllowAdd=false, AllowEdit=false, AllowDelete=false — showAddButton: false in JS
        // Explicitly built in controller Index() — never left as new()
        public DataGridConfig<DepartmentIncomeSnapshotItem> SnapshotGrid { get; set; } = new();
    }

    /// <summary>
    /// Grid row model for the Snapshot-tab DataGrid (JS getPeriodColumns() — 3 columns).
    /// Matches the snapshotData structure from fps_department_income.js:
    ///   periodName (240px, ReadOnly), finalSummariesRun (180px, Checkbox), periodLocke (150px, Checkbox)
    /// The snapshot data also carries projectCode and month (hidden, used for filtering in JS).
    /// </summary>
    // TRANSFORMENGINE: Snapshot grid row model — maps to JS snapshotData rows (not one of the 5 query types)
    public class DepartmentIncomeSnapshotItem
    {
        // TRANSFORMENGINE: Hidden row key — projectCode from snapshotData; used for project filter, not displayed
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? ProjectCode { get; set; }

        // TRANSFORMENGINE: Hidden month value — from snapshotData.month; used for period filter, not displayed
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Month { get; set; }

        // TRANSFORMENGINE: JS getPeriodColumns() — field: 'periodName', header: 'Period Name', width: 240
        [Display(Name = "Period Name")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? PeriodName { get; set; }

        // TRANSFORMENGINE: JS getPeriodColumns() — field: 'finalSummariesRun', header: 'Final Summaries Run', width: 180
        // render: renderBooleanCheckbox — GridColumnType.Checkbox
        [Display(Name = "Final Summaries Run")]
        [GridColumn(Width = 180, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool FinalSummariesRun { get; set; }

        // TRANSFORMENGINE: JS getPeriodColumns() — field: 'periodLocke', header: 'Period Locke', width: 150
        // render: renderBooleanCheckbox — GridColumnType.Checkbox
        [Display(Name = "Period Locke")]
        [GridColumn(Width = 150, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool PeriodLocke { get; set; }
    }
}
