/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Resource Centre (Profit Centre) Manager link sub-grid
 *     (Manager Tab, gridContainer_mgrResourceTable)
 *   - Composite natural PK: Profitcentre (string) + Manager (string); no PUT/update operation
 *   - AllowAdd=true (btnAddMgrResource), AllowEdit=false (no update on composite link),
 *     AllowDelete=true
 *   - Columns derived from HTML mgrAssignEditModal (reused for Resource Centre): mgrAssignValue
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.ProfitCentreManagerLinkDto exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<ProfitCentreManagerLinkItem, ProfitCentreManagerLinkDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for Resource Centre Manager link sub-grid (Manager Tab)
    // Composite natural PK: Profitcentre + Manager — both visible columns
    public class ProfitCentreManagerLinkItem
    {
        // TRANSFORMENGINE: Profit centre name — part of composite PK; visible column
        [Required(ErrorMessage = "Resource Centre is required")]
        [Display(Name = "Resource Centre")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Profitcentre { get; set; }

        // TRANSFORMENGINE: Manager name — part of composite PK; populated from selected manager context
        [Display(Name = "Manager")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }
    }
}
