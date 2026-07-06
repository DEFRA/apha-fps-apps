/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the RadTrackProg (PIMS Programmes) grid (Programme Tab,
 *     gridContainer_pimsProgTable)
 *   - Columns derived from HTML progProgEditModal: progProgName (Programme select),
 *     progProgPrefix (Publication Prefix text input)
 *   - AllowAdd=true (btnAddProg), AllowEdit=true (edit button), AllowDelete=true
 *   - Natural string PK: Program
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.RadTrackProgDto exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<RadTrackProgItem, RadTrackProgDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for PIMS Programmes grid (Programme Tab)
    // Natural string PK: Program — visible column and KeyProperty
    public class RadTrackProgItem
    {
        // TRANSFORMENGINE: Natural varchar PK — visible and KeyProperty; "Programme" in progProgEditModal
        [Required(ErrorMessage = "Programme is required")]
        [Display(Name = "Programme")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Program { get; set; }

        // TRANSFORMENGINE: RadTrack flag — boolean indicating this is a RadTrack programme
        [Display(Name = "RadTrack")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Radtrackprog { get; set; }

        // TRANSFORMENGINE: Publication prefix — "Publication Prefix" in progProgEditModal (progProgPrefix)
        [Display(Name = "Publication Prefix")]
        [GridColumn(Order = 3, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Publicationprefix { get; set; }
    }
}
