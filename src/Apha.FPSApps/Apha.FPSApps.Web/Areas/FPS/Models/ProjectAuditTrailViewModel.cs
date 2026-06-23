/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — page ViewModel for the Project Audit Trail tabbed view
 *   - 5 DataGridConfig properties (one per tab): ProjectLogsGrid, StaffJobLogsGrid,
 *     TestRequirementLogsGrid, AnimalRequestLogsGrid, AdditionalCostLogsGrid
 *   - Filter properties matching HTML prototype: ParentProject (select id=filter-project),
 *     FromDate (input id=filter-from), ToDate (input id=filter-to)
 *   - ProjectList dropdown — sourced from IProjectService.GetAllProjectsAsync()
 *     to match HTML filter-project <select> element (explicit <select> outside the grid container)
 *
 * PRESERVED:
 *   - HTML prototype filter structure: one project <select> + two date inputs outside all grids
 *   - Tab structure: Project Detail Changes, Staff Plan Changes, Test Requirement Changes,
 *     Animal Requirement Changes, Exceptional Cost Changes (5 tabs)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm FromDate/ToDate should remain DateOnly? at the ViewModel
 *     boundary (matching IProjectAuditTrailService param types) or convert to string? for
 *     HTML date input binding.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Page ViewModel for the Project Audit Trail tabbed view.
    /// Holds filter state (ParentProject, FromDate, ToDate), project dropdown list,
    /// and one DataGridConfig per audit log tab.
    /// All grids are read-only (AllowAdd=false, AllowEdit=false, AllowDelete=false) per prototype.
    /// </summary>
    public class ProjectAuditTrailViewModel
    {
        // TRANSFORMENGINE: Filter — HTML <select id="filter-project"> outside all grid containers
        public string? ParentProject { get; set; }

        // TRANSFORMENGINE: Filter — HTML <input id="filter-from" type="date"> outside all grid containers
        public DateOnly? FromDate { get; set; }

        // TRANSFORMENGINE: Filter — HTML <input id="filter-to" type="date"> outside all grid containers
        public DateOnly? ToDate { get; set; }

        // TRANSFORMENGINE: Dropdown list for ParentProject filter select element
        // Populated in controller via IProjectService.GetAllProjectsAsync()
        public List<SelectListItem> ProjectList { get; set; } = new();

        // TRANSFORMENGINE: Tab 1 — Project Detail Changes grid (initializeProjectAuditTrailTable)
        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<ProjectLogItem> ProjectLogsGrid { get; set; } = new();

        // TRANSFORMENGINE: Tab 2 — Staff Plan Changes grid (initializeStaffPlanChangesTable)
        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<StaffJobLogItem> StaffJobLogsGrid { get; set; } = new();

        // TRANSFORMENGINE: Tab 3 — Test Requirement Changes grid (initializeTestRequirementChangesTable)
        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<TestRequirementLogItem> TestRequirementLogsGrid { get; set; } = new();

        // TRANSFORMENGINE: Tab 4 — Animal Requirement Changes grid (initializeAnimalRequirementChangesTable)
        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<AnimalRequestLogItem> AnimalRequestLogsGrid { get; set; } = new();

        // TRANSFORMENGINE: Tab 5 — Exceptional Cost Changes grid (initializeExceptionalCostChangesTable)
        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<AdditionalCostLogItem> AdditionalCostLogsGrid { get; set; } = new();
    }
}
