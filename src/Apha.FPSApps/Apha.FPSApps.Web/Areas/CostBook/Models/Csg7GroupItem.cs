/*
 * TRANSFORMENGINE MIGRATION — Csg7GroupItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend grid Item model created for frmMaintainance Tab 3 (CSG7 Inflation Options)
 *   - Column definitions derived from JS initializeCsg7Grid DataGridComponent:
 *       csg7Group (240px), useInflation (150px, render: value ? "Yes" : "No")
 *   - showAddButton: true → AllowAdd = true
 *   - onEdit callback present → AllowEdit = true
 *   - onDelete callback present → AllowDelete = true
 *   - Csg7Group is the natural string PK for backend CRUD routes (varchar 15)
 *   - Csg7Group is a visible JS column — NOT hidden despite being the key property
 *   - UseInflation renders as "Yes"/"No" text in the grid (not a ✔ checkbox render)
 *     → GridColumnType.ReadOnly in grid; editable via modal checkbox (modal-csg7-useinflation)
 *
 * PRESERVED:
 *   - Both visible JS columns mapped in order
 *   - Column widths match JS definitions exactly
 *   - UseInflation bool type preserved for modal binding and DTO mapping
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) client-side validation applied in modal
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

// TRANSFORMENGINE: Grid item model for frmMaintainance Tab 3 (CSG7 Inflation Options)
//   JS grid: csg7Grid — showAddButton: true, onEdit: present, onDelete: present
//   KeyProperty = "Csg7Group" (route key for PUT /accountgroup/{csg7Group})
public class Csg7GroupItem
{
    // TRANSFORMENGINE: JS column field="csg7Group", header="CSG7 Group", width=240
    //   Maps to AccountGroupDto.Csg7Group (PK, varchar 15)
    //   Visible JS column — NOT hidden despite being the key property
    [Required(ErrorMessage = "CSG7 Group is required.")]
    [Display(Name = "CSG7 Group")]
    [GridColumn(Order = 1, Width = 240, Type = GridColumnType.Text, IsFilterable = true)]
    public string Csg7Group { get; set; } = null!;

    // TRANSFORMENGINE: JS column field="useInflation", header="Use Inflation?", width=150
    //   render: function(value) { return value ? "Yes" : "No"; } — text render, not ✔ checkbox
    //   GridColumnType.ReadOnly because the grid displays text "Yes"/"No"; edit is via modal checkbox
    //   Maps to AccountGroupDto.UseInflation (bool)
    [Display(Name = "Use Inflation?")]
    [GridColumn(Order = 2, Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = false)]
    public bool UseInflation { get; set; }
}
