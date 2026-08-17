using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class QueryResultItem
    {
        [Display(Name = "Year")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.ReadOnly)]
        public short? Year { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.ReadOnly)]
        public string? Project { get; set; }

        [Display(Name = "ParentProject")]
        [GridColumn(Order = 3, Width = 130, Type = GridColumnType.ReadOnly)]
        public string? ParentProject { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Order = 4, Width = 110, Type = GridColumnType.ReadOnly)]
        public string? Program { get; set; }

        [Display(Name = "Project Title")]
        [GridColumn(Order = 5, Width = 220, Type = GridColumnType.ReadOnly)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 6, Width = 180, Type = GridColumnType.ReadOnly)]
        public string? Manager { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Order = 7, Width = 120, Type = GridColumnType.ReadOnly)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Order = 8, Width = 110, Type = GridColumnType.ReadOnly)]
        public string? Contract { get; set; }

        [Display(Name = "Total Plan Costs")]
        [GridColumn(Order = 9, Width = 140, Type = GridColumnType.GbpValue)]
        public decimal? TotalPlanCosts { get; set; }

        [Display(Name = "Total YTD Costs")]
        [GridColumn(Order = 10, Width = 140, Type = GridColumnType.GbpValue)]
        public decimal? TotalYtdCosts { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 11, Width = 220, Type = GridColumnType.ReadOnly)]
        public string? MonitoringComment { get; set; }
    }
}