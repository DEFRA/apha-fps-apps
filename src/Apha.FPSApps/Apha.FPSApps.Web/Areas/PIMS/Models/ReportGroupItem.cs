/*
 * TRANSFORMENGINE MIGRATION — ReportGroupItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Report Groups sub-grid (gridContainer_reportGroupsTable, Reports Tab)
 *   - Columns derived from HTML group edit modal (groupEditModal): Groupid, Description
 *   - AllowAdd=true (btnAddReportGroup present), AllowEdit=true, AllowDelete=true
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.ReportGroupDto exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<ReportGroupItem, ReportGroupDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for Report Groups sub-grid (Reports Tab, gridContainer_reportGroupsTable)
    public class ReportGroupItem
    {
        // TRANSFORMENGINE: Numeric PK — KeyProperty for edit/delete operations
        [Display(Name = "Group ID")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
        public int Groupid { get; set; }

        // TRANSFORMENGINE: "Report Group" in groupEditModal — group description/name
        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 300, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }
    }
}
