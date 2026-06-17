using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectDetailsGridItem
    {
        //[GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        //public string ParentProject { get; set; } = null!;

        [Display(Name = "Project")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, Order = 1, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Project Title")]
        [GridColumn(Width = 220, Type = GridColumnType.ReadOnly, Order = 2, IsFilterable = true)]
        public string ProjectTitle { get; set; } = string.Empty;

        [Display(Name = "Program")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, Order = 3, IsFilterable = false)]
        public string? Program { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, Order = 4, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, Order = 5, IsFilterable = false)]
        public string? Customer { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, Order = 6, IsFilterable = false)]
        public string? Contract { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, Order = 7, IsFilterable = false)]
        public string? Status { get; set; }

        [Display(Name = "Transfer Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, Order = 8)]
        public decimal TransferIncome { get; set; }

        [Display(Name = "Cust Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, Order = 9)]
        public decimal CustIncome { get; set; }

        [Display(Name = "Budget")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, Order = 10)]
        public decimal? Budget { get; set; }
    }
}
