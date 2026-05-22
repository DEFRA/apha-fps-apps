using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupTimeCodeItem
    {
        [Display(Name = "PACT Staff ID")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? PACTStaffID { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text, IsFilterable =true)]
        public string? Name { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Parent Project")]
        [GridColumn(Order = 4, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Time Code")]
        [GridColumn(Order = 5, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Month")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public double Month { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Order = 7, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public double Hours { get; set; }
    }
}
