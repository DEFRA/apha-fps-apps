/*
 * TRANSFORMENGINE MIGRATION — CostCentreItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - Updated from Phase 10 scaffold to Phase 11 finalised grid item + modal partial model
 *   - [Required] added to ProfitCentre: JS costCentreValidationFields includes modal-cc-profit
 *     with message 'Select ProfitCentre' — server-side validation must mirror JS modal validation
 *   - GridColumn attributes confirmed against costcenter_maintenance.js columns array:
 *       column[0]: field='costCentre', header='Cost Centre', width=180 → CostCentreNo ReadOnly Width=140
 *       column[1]: field='profitCentre', header='Profit Centre', width=220 → ProfitCentre ReadOnly Width=200
 *   - FpsYear: hidden non-displayed composite PK component; managed server-side via X-FPS-Year header
 *
 * PRESERVED:
 *   - Property names CostCentreNo and ProfitCentre match CostCentreDto exactly for
 *     convention-based AutoMapper mapping in FpsViewModelMapper
 *   - GridColumnType.ReadOnly on both visible columns — grid is display-only;
 *     Add/Edit occur via CRUD modal (_AddEditCostCentre.cshtml), not inline editing
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: JS modal-cc-number select (cost centre number) uses a populated
 *     <select> in the HTML prototype — Phase 12 modal partial must determine whether CostCentreNo
 *     is a free-text input or a lookup-driven dropdown; verify Add vs Edit modal field behaviour
 *   - TRANSFORMENGINE TODO: Widths (140, 200) derive from Phase 10 estimation; adjust to match
 *     final Razor DataGrid column widths when Phase 12 view is validated in browser
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// DataGrid row item and Add/Edit modal partial model for Cost Centre Maintenance.
    /// Properties derived from costcenter_maintenance.js DataGridComponent columns array.
    /// Property names must match <c>CostCentreDto</c> exactly for AutoMapper convention mapping.
    /// </summary>
    public class CostCentreItem
    {
        // TRANSFORMENGINE: JS column[0] field='costCentre', header='Cost Centre', width=180
        //   fps.costcentre.costcentre double precision — composite primary key component.
        //   Visible in grid (JS column present); used as KeyProperty in DataGridConfig.
        //   double is a value type — [Required] not applicable; implicitly required.
        [Display(Name = "Cost Centre")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public double CostCentreNo { get; set; }

        // TRANSFORMENGINE: JS column[1] field='profitCentre', header='Profit Centre', width=220
        //   fps.costcentre.profitcentre varchar(50) — FK to fps.tblkpprofitcentre.
        //   [Required] mirrors JS costCentreValidationFields[1]: id='modal-cc-profit',
        //   message='Select ProfitCentre' — server-side validation enforces JS modal rule.
        [Required(ErrorMessage = "Profit Centre is required")]
        [Display(Name = "Profit Centre")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProfitCentre { get; set; } = string.Empty;

        // TRANSFORMENGINE: FpsYear — composite PK component; NOT a visible JS column.
        //   Hidden field; FPS financial year partition managed server-side via HasQueryFilter.
        //   Included for full AutoMapper round-trip with CostCentreDto.FpsYear.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
