/*
 * TRANSFORMENGINE MIGRATION — FrequencyItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Frequency lookup grid (Other Tab)
 *   - Columns derived from HTML otherEditModal fields: otherEditId (ID), otherEditValue (Value)
 *   - AllowAdd=true, AllowEdit=true, AllowDelete=true (edit and delete buttons in otherDataRowTemplate)
 *   - Integer PK: Frequencyid
 *   - FrequencyValue maps from backend Frequency field via ForMember in PimsMaintenanceApiDtoMapper
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.FrequencyDto exactly
 *   - FrequencyValue name (mapped from backend Frequency column)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<FrequencyItem, FrequencyDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for Frequency lookup table (Other Tab)
    // Integer PK: Frequencyid; FrequencyValue name maps from backend Frequency column
    public class FrequencyItem
    {
        // TRANSFORMENGINE: Integer PK — visible column and KeyProperty
        [Display(Name = "ID")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Frequencyid { get; set; }

        // TRANSFORMENGINE: Frequency value — maps from backend Frequency column via ForMember
        // "Value" label in otherEditModal (otherEditValue)
        [Required(ErrorMessage = "Frequency value is required")]
        [Display(Name = "Frequency")]
        [GridColumn(Order = 2, Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? FrequencyValue { get; set; }
    }
}
