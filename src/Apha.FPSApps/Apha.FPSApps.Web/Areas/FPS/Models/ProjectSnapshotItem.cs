using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Snapshot Projects DataGrid.
    /// Property names match <c>ProjectSnapshotViewDto</c> for AutoMapper convention mapping.
    /// </summary>
    public class ProjectSnapshotItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Project Title")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProjectTitle { get; set; } = null!;

        [Display(Name = "Program")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Customer { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "Transfer Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal TransferIncome { get; set; }

        [Display(Name = "Budget CVL")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Budget Ext")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? BudgetExt { get; set; }

        [Display(Name = "Customer Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal CustIncome { get; set; }

        [Display(Name = "PVS Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? PvsIncome { get; set; }

        [Display(Name = "WIP EOY")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? WipEoy { get; set; }

        [Display(Name = "WIP Limit")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? WipLimit { get; set; }

        [Display(Name = "WIP Current")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? WipCurrent { get; set; }

        [Display(Name = "FEC Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? FecCost { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Profit")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Profit { get; set; }

        [Display(Name = "Disease")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Disease { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }
    }
}
