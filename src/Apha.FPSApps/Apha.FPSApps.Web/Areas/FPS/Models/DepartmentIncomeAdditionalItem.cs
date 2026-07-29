using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // qryDeptIncomeAdditional (VBA: qryDeptIncomeExceptional) — 8 columns (aggregated exceptional costs)
    public class DepartmentIncomeAdditionalItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        [Display(Name = "OracleProject")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        [Display(Name = "SubAccount")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SubAccountCode { get; set; }

        [Display(Name = "DefraProject")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? DefraProject { get; set; }

        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        // Maps to JS 'totalCost' column (width: 110)
        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
