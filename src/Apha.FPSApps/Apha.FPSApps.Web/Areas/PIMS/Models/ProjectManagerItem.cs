/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New DataGrid row item for the Manager grid (Manager Tab, gridContainer_mgrTable)
 *   - Columns derived from HTML mgrManagerEditModal: Projectmanager (name), Email, Mnumber, Disable
 *   - AllowAdd=true (btnAddManager present), AllowEdit=true, AllowDelete=true
 *   - Projectmanager is natural varchar PK (string key)
 *
 * PRESERVED:
 *   - Field names match Apha.FPSApps.Application.Dtos.PIMS.ProjectManagerDto exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<ProjectManagerItem, ProjectManagerDto>().ReverseMap() to
 *     PimsMaintenanceViewModelMapper once this type is registered
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Grid row item for Manager grid (Manager Tab, gridContainer_mgrTable)
    // Natural string PK — Projectmanager is both the key and a visible column
    public class ProjectManagerItem
    {
        // TRANSFORMENGINE: Natural varchar PK — visible in grid and also KeyProperty
        [Required(ErrorMessage = "Manager name is required")]
        [Display(Name = "Manager Name")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Projectmanager { get; set; }

        // TRANSFORMENGINE: "Manager's email" in mgrManagerEditModal (mgrEditEmail)
        [Display(Name = "Email")]
        [GridColumn(Order = 2, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Email { get; set; }

        // TRANSFORMENGINE: "MNumber" in mgrManagerEditModal (mgrEditMNumber)
        [Display(Name = "MNumber")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Mnumber { get; set; }

        // TRANSFORMENGINE: "Disable" checkbox in mgrManagerEditModal (mgrEditDisabled)
        [Display(Name = "Disabled")]
        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Disable { get; set; }
    }
}
