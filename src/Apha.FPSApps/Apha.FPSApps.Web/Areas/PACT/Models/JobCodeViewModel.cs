using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class JobCodeViewModel
    {
        [Display(Name = "Job Code")]
        [Required(ErrorMessage = "Job Code is required")]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 296, Type = GridColumnType.Text, IsFilterable = true)]
        public string JobCodeId { get; set; } = null!;

        [Display(Name = "Parent Project")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string? ParentProject { get; set; }

        [Display(Name = "Work Group")]
        [StringLength(50)]
        [GridColumn(Order = 4, Width = 296, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCodeWorkGroup { get; set; }

        [Display(Name = "Type")]
        [Required(ErrorMessage = "Type is required")]
        [StringLength(15)]
        [GridColumn(Order = 3, Width = 296, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Type { get; set; }

        [Display(Name = "Job Code Name")]
        [StringLength(255)]
        [GridColumn(Order = 2, Width = 296, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCodeName { get; set; }
    }
}
