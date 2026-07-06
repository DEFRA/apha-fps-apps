/*
 * TRANSFORMENGINE MIGRATION — SettingItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Settings grid (Time Tab)
 *   - Columns derived from HTML Time Tab form fields: timeWorkingHours, timeWorkingDays
 *   - Setting is read/update only — no Create/Delete operations
 *   - AllowAdd=false (no add button in Time Tab), AllowEdit=true (save button), AllowDelete=false
 *   - String PK: Id (setting key)
 *   - NOTE: Time Tab uses a custom form layout (not a standard DataGrid), but SettingItem
 *     provides the data model for Settings CRUD modal and ViewModel binding
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.SettingDto exactly
 *   - SettingValue name (mapped from backend Setting field via ForMember in PimsMaintenanceApiDtoMapper)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<SettingItem, SettingDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Data model for Settings (Time Tab form fields — timeWorkingHours, timeWorkingDays)
    // Used for direct form binding rather than a standard DataGrid
    public class SettingItem
    {
        // TRANSFORMENGINE: String PK — setting key identifier (e.g. "WorkingHours", "WorkingDays")
        [Display(Name = "Setting ID")]
        [GridColumn(Order = 1, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Id { get; set; }

        // TRANSFORMENGINE: Setting value mapped from backend Setting field (ForMember in PimsMaintenanceApiDtoMapper)
        [Display(Name = "Value")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string? SettingValue { get; set; }

        // TRANSFORMENGINE: Notes/description field
        [Display(Name = "Notes")]
        [GridColumn(Order = 3, Width = 280, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Notes { get; set; }

        // TRANSFORMENGINE: Test setting value — backend-only; hidden in UI
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Testsetting { get; set; }

        // TRANSFORMENGINE: Whether this setting is user-updateable
        [Display(Name = "User Updateable")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Userupdateable { get; set; }
    }
}
