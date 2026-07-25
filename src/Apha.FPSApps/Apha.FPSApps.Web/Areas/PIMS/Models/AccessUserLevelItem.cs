using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AccessUserLevelItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SystemId { get; set; }

        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        [GridColumn(Order = 1, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        public string? NtLogin { get; set; }

        [Required(ErrorMessage = "Access Level is required")]
        [Display(Name = "Access Level")]
        [GridColumn(Order = 2, Width = 180, Type = GridColumnType.Number, IsFilterable = true)]
        public int AccessLevelId { get; set; }

        [Display(Name = "Access Level Name")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AccessLevelName { get; set; }

        [GridColumn(IsVisible = false)]
        public string CompositeKey => $"{NtLogin}|{AccessLevelId}|{SystemId}";
    }
}
