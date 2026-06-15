// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New grid row Item class derived from the JS DataGridComponent columns array in
 *     source/ui/fps/projectprofitability_vla.js (initializeProjectProfitabilityVlaTable).
 *   - 12 visible grid columns mapped from JS columns[]: project, program, customer,
 *     staffCosts, testCost, animal, addCosts, totalCosts, budget, profit,
 *     targetProfit, offTarget.
 *   - 'animal' JS field mapped to DTO property AnimalCosts (consistent with frontend DTO
 *     field name); Display Name set to "Animal" to match the JS header.
 *   - 'addCosts' JS field mapped to DTO property AdditionalCosts; Display Name "Add Costs".
 *   - 'project' JS field mapped to DTO property JobCode; Display Name "Project" matching JS header.
 *   - All currency fields use GridColumnType.GbpValueRounded (matches existing
 *     ProjectProfitabilityItem pattern and projectprofitability_vla.js formatCurrency behaviour).
 *   - manager and status are NOT grid columns (absent from JS columns[]) — hidden helper
 *     properties retained for filter state passing only.
 *   - Id hidden (not a visible JS column; used as KeyProperty row discriminator only).
 *   - No [Required] attributes — this is a read-only view-only grid (AllowAdd/Edit/Delete = false).
 *
 * PRESERVED:
 *   - All property names exactly match Apha.FPSApps.Application.Dtos.FPS.ProjectProfitabilityVlaDto
 *     to satisfy the convention AutoMapper binding registered in FpsViewModelMapper Phase 10.
 *   - OffTarget field retains GbpValueRounded type; negative value CSS highlight is applied
 *     in the Razor view (mirrors projectprofitability_vla.js fps-profit-offtarget behaviour).
 *   - Budget nullable (decimal?) matching frontend DTO nullability.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: if FpsViewModelMapper requires a ForMember for JobCode→Project
 *     display, add it there; the Item property name JobCode is intentional (DTO alignment).
 *   - TRANSFORMENGINE TODO: confirm Budget nullable (decimal?) is correct once the
 *     vprojectprofitability view DDL is finalised.
 *   - TRANSFORMENGINE TODO: confirm Id nullability (int?) once view DDL is finalised.
 *   - TRANSFORMENGINE TODO: verify OffTarget negative-value red highlight is implemented in
 *     the _DataGrid partial or Index.cshtml Razor view (fps-profit-offtarget CSS class).
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Project Profitability VLA DataGrid.
    /// Derived from the JS DataGridComponent columns array in projectprofitability_vla.js.
    /// Property names must exactly match <c>ProjectProfitabilityVlaDto</c> for AutoMapper
    /// convention mapping registered in <c>FpsViewModelMapper</c>.
    /// </summary>
    public class ProjectProfitabilityVlaItem
    {
        // TRANSFORMENGINE: hidden row discriminator — Id not a visible JS column;
        //   used as KeyProperty only. Nullable int? mirrors frontend DTO.
        [GridColumn(IsVisible = false)]
        public int? Id { get; set; }

        // TRANSFORMENGINE: JS column[0] field='project', header='Project', width=120
        //   — DTO property is JobCode; Display Name "Project" matches the JS header.
        //   Convention AutoMapper maps ProjectProfitabilityVlaDto.JobCode → JobCode here.
        [Display(Name = "Project")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: JS column[1] field='program', header='Program', width=120
        [Display(Name = "Program")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        // TRANSFORMENGINE: JS column[2] field='customer', header='Customer', width=160
        [Display(Name = "Customer")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Customer { get; set; }

        // TRANSFORMENGINE: JS column[3] field='staffCosts', header='Staff Costs', width=130 — currency
        [Display(Name = "Staff Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal StaffCosts { get; set; }

        // TRANSFORMENGINE: JS column[4] field='testCost', header='Test Cost', width=120 — currency
        [Display(Name = "Test Cost")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal TestCost { get; set; }

        // TRANSFORMENGINE: JS column[5] field='animal', header='Animal', width=110 — currency
        //   DTO property name is AnimalCosts (not 'animal'); Display Name "Animal" matches JS header.
        [Display(Name = "Animal")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal AnimalCosts { get; set; }

        // TRANSFORMENGINE: JS column[6] field='addCosts', header='Add Costs', width=120 — currency
        //   DTO property name is AdditionalCosts (not 'addCosts'); Display Name "Add Costs" matches JS header.
        [Display(Name = "Add Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal AdditionalCosts { get; set; }

        // TRANSFORMENGINE: JS column[7] field='totalCosts', header='Total Costs', width=130 — currency
        [Display(Name = "Total Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: JS column[8] field='budget', header='Budget', width=120 — currency; nullable
        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal? Budget { get; set; }

        // TRANSFORMENGINE: JS column[9] field='profit', header='Profit', width=110 — currency
        [Display(Name = "Profit")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal Profit { get; set; }

        // TRANSFORMENGINE: JS column[10] field='targetProfit', header='Target Profit', width=130 — currency
        [Display(Name = "Target Profit")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal TargetProfit { get; set; }

        // TRANSFORMENGINE: JS column[11] field='offTarget', header='Off-Target', width=130 — currency
        //   Negative value triggers red highlight (fps-profit-offtarget CSS class) in Razor view.
        [Display(Name = "Off-Target")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal OffTarget { get; set; }

        // TRANSFORMENGINE: manager and status are filter-only fields (absent from JS columns[]);
        //   retained as hidden properties to allow round-trip filter state if needed.
        [GridColumn(IsVisible = false)]
        public string? Manager { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Status { get; set; }
    }
}
