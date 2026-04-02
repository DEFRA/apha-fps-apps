using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectJobCodeViewModel
    {
        [Display(Name = "Job Code")]
        [Required]
        [StringLength(50)]
        [GridColumn(Width = 296, Type = GridColumnType.Text, IsFilterable = true)]
        public string JobCodeId { get; set; } = null!;

        [Display(Name = "Parent Project")]
        [StringLength(50)]
        [GridColumn(Width = 923, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ParentProject { get; set; }

        [Display(Name = "Work Group")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string? JobCodeWorkGroup { get; set; }

        [Display(Name = "Type")]
        [StringLength(15)]
        [GridColumn(IsVisible = false)]
        public string? Type { get; set; }

        [Display(Name = "Job Code Name")]
        [StringLength(255)]
        [GridColumn(IsVisible = false)]
        public string? JobCodeName { get; set; }
    }
}
