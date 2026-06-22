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
