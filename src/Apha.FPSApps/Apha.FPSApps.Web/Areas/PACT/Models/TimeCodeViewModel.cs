using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TimeCodeViewModel
    {
        [Display(Name = "Time Code")]
        [Required]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Work Group")]
        [Required]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Parent Project")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Job Code")]
        [StringLength(50)]
        [GridColumn(Order = 3, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Active")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Active { get; set; }

        /// <summary>
        /// Holds the WorkGroup value as it existed before editing.
        /// Required to locate and replace the old composite key record
        /// (ParentProject + OriginalWorkGroup + TimeCode) when WorkGroup changes.
        /// Not rendered in the grid.
        /// </summary>
        [GridColumn(IsVisible = false)]
        public string? OriginalWorkGroup { get; set; }
    }
}
