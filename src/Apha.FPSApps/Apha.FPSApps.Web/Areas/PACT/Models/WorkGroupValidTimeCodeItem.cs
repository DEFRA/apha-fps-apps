using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupValidTimeCodeItem
    {
        [Display(Name = "Worker")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "TimeCode")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TimeCode { get; set; }
        
        [Display(Name = "Active")]
        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Active { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Order = 5, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ParentProject { get; set; }
    }
}