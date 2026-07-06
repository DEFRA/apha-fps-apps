/*
 * TRANSFORMENGINE MIGRATION — AccessUserItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Users grid (Admin Maintenance Tab, gridContainer_adminUsersTable)
 *   - Columns derived from admin.js DataGridComponent:
 *       { field: "ntlogin", header: "NTLogin", width: 170 }
 *       { field: "username", header: "UserName", width: 240 }
 *       actions column with edit + delete buttons
 *   - AllowAdd=true (btnAddUser), AllowEdit=true (edit button), AllowDelete=true (delete button)
 *   - Composite PK: Systemid (int) + Ntlogin (string)
 *   - Systemid is backend-managed — hidden in grid; Ntlogin is visible as first column
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.AccessUserDto exactly
 *   - admin.js modal fields: adminUserNTLogin, adminUserName preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<AccessUserItem, AccessUserDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for Users grid (Admin Maintenance Tab)
    // Composite PK: Systemid + Ntlogin; Ntlogin is visible column per admin.js columns array
    public class AccessUserItem
    {
        // TRANSFORMENGINE: Systemid — backend-managed part of composite PK; hidden in grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Systemid { get; set; }

        // TRANSFORMENGINE: NTLogin — visible column per admin.js { field: "ntlogin", header: "NTLogin", width: 170 }
        [Required(ErrorMessage = "NTLogin is required")]
        [Display(Name = "NTLogin")]
        [GridColumn(Order = 1, Width = 170, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Ntlogin { get; set; }

        // TRANSFORMENGINE: UserName — visible column per admin.js { field: "username", header: "UserName", width: 240 }
        [Required(ErrorMessage = "UserName is required")]
        [Display(Name = "UserName")]
        [GridColumn(Order = 2, Width = 240, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Username { get; set; }

        // TRANSFORMENGINE: Dt2login — additional login field from DTO; hidden in grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Dt2login { get; set; }
    }
}
