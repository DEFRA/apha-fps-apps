/*
 * TRANSFORMENGINE MIGRATION — ReportItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item and modal partial model for the Reports grid (Reports Tab)
 *   - Columns derived from frmMaintainance.html maintenance edit modal fields and ReportDto shape
 *   - All modal fields preserved: Reportname, Reportdescription, Reporthelp, Mailcomment,
 *     Mailtitle, Sortorder, Emailable (checkbox)
 *   - AllowAdd=true (btnAddReport in HTML), AllowEdit=true (edit button in actions),
 *     AllowDelete=true (delete button in actions)
 *
 * PRESERVED:
 *   - All DTO field names match Apha.FPSApps.Application.Dtos.PIMS.ReportDto exactly
 *   - Boolean Allowpick* fields included for full round-trip compatibility
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: maintenance.js was not present in source/ui/pims/; column widths
 *     derived from HTML modal context — verify against real DataGridComponent column spec
 *   - TRANSFORMENGINE TODO: Add CreateMap<ReportItem, ReportDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item + modal partial model for Reports grid (Reports Tab, gridContainer_maintenanceTable)
    public class ReportItem
    {
        // TRANSFORMENGINE: Hidden PK — not a visible JS grid column; used as KeyProperty for edit/delete
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Id { get; set; }

        // TRANSFORMENGINE: Primary display field — "Name" in maintenance edit modal (editName)
        [Required(ErrorMessage = "Report name is required")]
        [Display(Name = "Name")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Reportname { get; set; }

        // TRANSFORMENGINE: "Description" in maintenance edit modal (editDescription)
        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Reportdescription { get; set; }

        // TRANSFORMENGINE: Type field from DTO — distinguishes report family
        [Display(Name = "Type")]
        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Type { get; set; }

        // TRANSFORMENGINE: Checkbox — "Email-able" in maintenance edit modal (editEmailable)
        [Display(Name = "Email-able")]
        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Emailable { get; set; }

        // TRANSFORMENGINE: Sort order numeric — "Order" in maintenance edit modal (editOrder)
        [Display(Name = "Order")]
        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.Number, IsFilterable = false)]
        public int? Sortorder { get; set; }

        // TRANSFORMENGINE: Long text — "Report Help" textarea in maintenance edit modal (editReportHelp)
        // Not shown in grid column; required for modal partial only
        [Required(ErrorMessage = "Report help is required")]
        [Display(Name = "Report Help")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Reporthelp { get; set; }

        // TRANSFORMENGINE: "Mail Comment" in maintenance edit modal (editMailComment)
        [Required(ErrorMessage = "Mail comment is required")]
        [Display(Name = "Mail Comment")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Mailcomment { get; set; }

        // TRANSFORMENGINE: "Mail Title" in maintenance edit modal (editMailTitle)
        [Required(ErrorMessage = "Mail title is required")]
        [Display(Name = "Mail Title")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Mailtitle { get; set; }

        // TRANSFORMENGINE: Filter string — backend-only field; not exposed in modal or grid
        [Display(Name = "Filter")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Filter { get; set; }

        // TRANSFORMENGINE: Boolean pick-option flags — carried through from ReportDto for full round-trip
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickprogramme { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickproject { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickmanager { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickcontract { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickcustomer { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickmonth { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool Allowpickfyear { get; set; }
    }
}
