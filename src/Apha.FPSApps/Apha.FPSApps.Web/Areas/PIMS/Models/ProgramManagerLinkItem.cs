/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLinkItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Program Manager link sub-grid
 *     (Manager Tab, gridContainer_mgrProgramTable)
 *   - Composite natural PK: Program (string) + Manager (string); no PUT/update operation
 *   - AllowAdd=true (btnAddMgrProgram), AllowEdit=false (no update on composite link),
 *     AllowDelete=true
 *   - Columns derived from HTML mgrAssignEditModal: mgrAssignValue (select — Programme value)
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.ProgramManagerLinkDto exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<ProgramManagerLinkItem, ProgramManagerLinkDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for Program Manager link sub-grid (Manager Tab)
    // Composite natural PK: Program + Manager — both visible columns
    public class ProgramManagerLinkItem
    {
        // TRANSFORMENGINE: Programme name — part of composite PK; visible column
        [Required(ErrorMessage = "Programme is required")]
        [Display(Name = "Programme")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Program { get; set; }

        // TRANSFORMENGINE: Manager name — part of composite PK; populated from selected manager context
        [Display(Name = "Manager")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }
    }
}
