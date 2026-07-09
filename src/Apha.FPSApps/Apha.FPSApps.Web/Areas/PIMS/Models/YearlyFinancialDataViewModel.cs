/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: frontend ViewModel for the Yearly Financial Data page
 *   - DataGridConfig<YearlyFinancialDataItem> CostCenterListGrid for the main per-project financial grid
 *   - Project dropdown sourced from IProjectListService.GetAllProjectsListAsync()
 *     (explicit <select id="yfdProject"> found OUTSIDE the grid container in frmProjectRadTrackData.html)
 *   - StartDate / EndDate read-only display fields bound to project selection
 *   - SelectedProject carries the current project context between requests
 *
 * PRESERVED:
 *   - All property names align exactly with frmProjectRadTrackData.html toolbar control ids
 *     (yfdProject → SelectedProject, yfdStartDate → StartDate, yfdEndDate → EndDate)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm StartDate/EndDate binding source —
 *     currently loaded from IProjectDetailsService.GetFpsProjectAsync() in the controller;
 *     verify the ProjectDto fields used (StartDate, RevisedEndDate or equivalent)
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    /// <summary>
    /// ViewModel for the Yearly Financial Data page (frmProjectRadTrackData).
    /// Contains the project selector dropdown, date display fields, and the DataGrid for per-year cost records.
    /// </summary>
    public class YearlyFinancialDataViewModel
    {
        // TRANSFORMENGINE: Toolbar scalar fields — mirrors yfdProject, yfdStartDate, yfdEndDate
        // in frmProjectRadTrackData.html toolbar

        /// <summary>Currently selected project code. Bound to <select id="yfdProject"> (explicit page-level filter).</summary>
        public string SelectedProject { get; set; } = string.Empty;

        /// <summary>Project start date — read-only display, populated by controller from project detail lookup.</summary>
        public string StartDate { get; set; } = string.Empty;

        /// <summary>Project (revised) end date — read-only display, populated by controller from project detail lookup.</summary>
        public string EndDate { get; set; } = string.Empty;

        // TRANSFORMENGINE: Project dropdown — explicit <select id="yfdProject"> OUTSIDE the grid container
        //                  in frmProjectRadTrackData.html → justified as a page-level filter dropdown

        /// <summary>Project list for the <select id="yfdProject"> dropdown.</summary>
        public List<SelectListItem> ProjectList { get; set; } = [];

        // TRANSFORMENGINE: DataGrid — id="gridContainer_costcenterList" in frmProjectRadTrackData.html
        //                  NEVER left as new() — built explicitly in controller Index()

        /// <summary>
        /// DataGrid configuration for the main yearly financial data grid.
        /// KeyProperty composite: Year is the discriminating key per row within a project.
        /// </summary>
        public DataGridConfig<YearlyFinancialDataItem> CostCenterListGrid { get; set; } = new();
    }
}
