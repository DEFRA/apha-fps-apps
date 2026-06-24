/*
 * TRANSFORMENGINE MIGRATION — CapsStaffItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend grid Item model created for frmMaintainance Tab 5 (CAPS Staff)
 *   - Column definitions derived from JS initializeCapsStaffGrid DataGridComponent:
 *       mNumber (160px), name (260px)
 *   - showAddButton: true → AllowAdd = true
 *   - onEdit callback present → AllowEdit = true
 *   - onDelete callback present → AllowDelete = true
 *   - MNumber is the natural string PK for backend CRUD routes (varchar 50)
 *   - Both MNumber and Name are visible JS columns
 *   - Dt2Number is not in JS grid columns — hidden field preserved for DTO mapping
 *
 * PRESERVED:
 *   - Both visible JS columns mapped in order
 *   - Column widths match JS definitions exactly
 *   - Dt2Number preserved as hidden (not in JS grid columns, not in HTML prototype modal)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number should be surfaced in add/edit modal
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

// TRANSFORMENGINE: Grid item model for frmMaintainance Tab 5 (CAPS Staff)
//   JS grid: capsStaffGrid — showAddButton: true, onEdit: present, onDelete: present
//   KeyProperty = "MNumber" (route key for PUT/DELETE /capsstaff/{mNumber})
public class CapsStaffItem
{
    // TRANSFORMENGINE: JS column field="mNumber", header="mNumber", width=160
    //   Maps to CapsStaffDto.MNumber (PK, varchar 50)
    //   Visible JS column — NOT hidden despite being the key property
    [Required(ErrorMessage = "mNumber is required.")]
    [Display(Name = "mNumber")]
    [GridColumn(Order = 1, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
    public string MNumber { get; set; } = null!;

    // TRANSFORMENGINE: JS column field="name", header="Name", width=260
    //   Maps to CapsStaffDto.Name (varchar 50, NOT NULL)
    [Required(ErrorMessage = "Name is required.")]
    [Display(Name = "Name")]
    [GridColumn(Order = 2, Width = 260, Type = GridColumnType.Text, IsFilterable = true)]
    public string Name { get; set; } = null!;

    // TRANSFORMENGINE: Dt2Number — not in JS grid columns array, not in HTML prototype modal
    //   Preserved as hidden field for DTO mapping round-trip; not displayed in grid or modal
    [GridColumn(IsVisible = false)]
    public string? Dt2Number { get; set; }
}
