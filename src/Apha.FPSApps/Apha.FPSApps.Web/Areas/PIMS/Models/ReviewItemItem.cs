/*
 * TRANSFORMENGINE MIGRATION — ReviewItemItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the ReviewItem lookup grid (Other Tab)
 *   - Columns derived from HTML otherEditModal fields: otherEditId (ID), otherEditValue (Value)
 *   - AllowAdd=true, AllowEdit=true, AllowDelete=true (edit and delete buttons in otherDataRowTemplate)
 *   - Integer PK: Itemid
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.ReviewItemDto exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<ReviewItemItem, ReviewItemDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for ReviewItem lookup table (Other Tab)
    // Integer PK: Itemid; Item is the description/value field
    public class ReviewItemItem
    {
        // TRANSFORMENGINE: Integer PK — visible column and KeyProperty
        [Display(Name = "ID")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Itemid { get; set; }

        // TRANSFORMENGINE: Review item description — "Value" label in otherEditModal (otherEditValue)
        [Required(ErrorMessage = "Item value is required")]
        [Display(Name = "Item")]
        [GridColumn(Order = 2, Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Item { get; set; }
    }
}
