using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupTimeCodeItem
    {
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "JobCode")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Surname")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public double Month { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public double Hours { get; set; }
    }
}
