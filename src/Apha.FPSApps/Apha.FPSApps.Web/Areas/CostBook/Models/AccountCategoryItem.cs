/*
 * TRANSFORMENGINE MIGRATION — AccountCategoryItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend grid Item model created for frmMaintainance Tab 2 (Account Categories)
 *   - Column definitions derived from JS initializeAccCatGrid DataGridComponent:
 *       accountShortName (220px), description (260px), csg7Group (180px)
 *   - showAddButton: true → AllowAdd = true
 *   - onEdit callback present → AllowEdit = true
 *   - onDelete callback present → AllowDelete = true
 *   - AccShortName is the natural PK for CRUD (matches backend route /account-categories/{accShortName})
 *   - AccShortName is a visible JS column (accountShortName) — kept as Text column, not hidden
 *   - Csg7Group is the only writable field on the backend PUT endpoint
 *   - AccountDescription is read-only (backend does not expose edit for description)
 *
 * PRESERVED:
 *   - All 3 visible JS columns mapped in order
 *   - Column widths match JS definitions exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear should be surfaced as a hidden/read-only column
 *   - TRANSFORMENGINE TODO: AccountDescription is read-only at the backend level — confirm no edit path needed
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

// TRANSFORMENGINE: Grid item model for frmMaintainance Tab 2 (Account Categories)
//   JS grid: accCatGrid — showAddButton: true, onEdit: present, onDelete: present
//   KeyProperty = "AccShortName" (route key for PUT /maintenance/account-categories/{accShortName})
public class AccountCategoryItem
{
    // TRANSFORMENGINE: JS column field="accountShortName", header="Account Short Name", width=220
    //   Maps to AccountCategoryMaintenanceDto.AccShortName (PK)
    //   Visible JS column — NOT hidden despite being the key property
    [Required(ErrorMessage = "Account Short Name is required.")]
    [Display(Name = "Account Short Name")]
    [GridColumn(Order = 1, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
    public string AccShortName { get; set; } = null!;

    // TRANSFORMENGINE: JS column field="description", header="Description", width=260
    //   Maps to AccountCategoryMaintenanceDto.AccountDescription — read-only (backend does not expose edit)
    [Display(Name = "Description")]
    [GridColumn(Order = 2, Width = 260, Type = GridColumnType.ReadOnly, IsFilterable = true)]
    public string? AccountDescription { get; set; }

    // TRANSFORMENGINE: JS column field="csg7Group", header="CSG7 Group", width=180
    //   Maps to AccountCategoryMaintenanceDto.Csg7Group — the only writable field in PUT endpoint
    [Display(Name = "CSG7 Group")]
    [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Dropdown, IsFilterable = true)]
    public string? Csg7Group { get; set; }

    // TRANSFORMENGINE: FpsYear — hidden PK component, not in JS columns array
    [GridColumn(IsVisible = false)]
    public int FpsYear { get; set; }
}
