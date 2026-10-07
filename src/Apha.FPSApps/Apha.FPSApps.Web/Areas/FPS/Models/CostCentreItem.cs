using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        //   fps.costcentre.costcentre double precision — composite primary key component.
        //   Visible in grid (JS column present); used as KeyProperty in DataGridConfig.
        //   Made nullable to enable [Required] validation for modal form input.
        [Display(Name = "CostCentre")]
        [Required(ErrorMessage = "CostCentre is required")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public double? CostCentreNo { get; set; }

        //   fps.costcentre.profitcentre varchar(50) — FK to fps.tblkpprofitcentre.
        //   [Required] mirrors JS costCentreValidationFields[1]: id='modal-cc-profit',
        //   message='Select ProfitCentre' — server-side validation enforces JS modal rule.
        [Required(ErrorMessage = "Profit Centre is required")]
        [Display(Name = "Profit Centre")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProfitCentre { get; set; } = string.Empty;

        //   Hidden field; FPS financial year partition managed server-side via HasQueryFilter.
        //   Included for full AutoMapper round-trip with CostCentreDto.FpsYear.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int FpsYear { get; set; }

        //   ProfitCentre dropdown options for the Add/Edit modal partial.
        //   Populated by the controller; not part of the AutoMapper round-trip with CostCentreDto.
        //   Hidden from the DataGrid (not a data column) to avoid rendering the list type as a cell value.
        [GridColumn(IsVisible = false)]
        public List<SelectListItem> ProfitCentreList { get; set; } = new();
    }
}
