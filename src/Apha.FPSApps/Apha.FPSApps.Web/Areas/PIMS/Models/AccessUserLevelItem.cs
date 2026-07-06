/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the User Access grid (Admin Maintenance Tab, gridContainer_adminAccessTable)
 *   - Columns derived from admin.js DataGridComponent (adminAccessGrid):
 *       { field: "user", header: "User", width: 220 }
 *       { field: "accessLevel", header: "AccessLevel", width: 180 }
 *       actions column with edit + delete buttons
 *   - AllowAdd=true (btnAddAccess), AllowEdit=true (edit button), AllowDelete=true (delete button)
 *   - Triple composite PK: Systemid (int) + Ntlogin (string) + Accesslevelid (int); no PUT
 *   - Note: admin.js uses "user" and "accessLevel" field names; these map to Ntlogin + Accesslevelid
 *     in the backend DTO. Username is included as display-only field resolved at render time.
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.AccessUserLevelDto exactly
 *   - admin.js adminAccessModal fields: adminAccessUser (select), adminAccessLevel (select)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<AccessUserLevelItem, AccessUserLevelDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for User Access grid (Admin Maintenance Tab)
    // Triple composite PK: Systemid + Ntlogin + Accesslevelid; no update (no PUT)
    public class AccessUserLevelItem
    {
        // TRANSFORMENGINE: Systemid — backend-managed part of composite PK; hidden
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Systemid { get; set; }

        // TRANSFORMENGINE: Ntlogin — user identifier (maps to "user" in admin.js access grid)
        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        [GridColumn(Order = 1, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Ntlogin { get; set; }

        // TRANSFORMENGINE: Accesslevelid — access level (maps to "accessLevel" in admin.js access grid)
        [Required(ErrorMessage = "Access Level is required")]
        [Display(Name = "Access Level")]
        [GridColumn(Order = 2, Width = 180, Type = GridColumnType.Number, IsFilterable = true)]
        public int Accesslevelid { get; set; }

        // TRANSFORMENGINE: Display-friendly access level name (resolved from AccessLevel lookup; not a DTO field)
        // Populated by controller for display purposes only
        [Display(Name = "Access Level Name")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AccessLevelName { get; set; }
    }
}
