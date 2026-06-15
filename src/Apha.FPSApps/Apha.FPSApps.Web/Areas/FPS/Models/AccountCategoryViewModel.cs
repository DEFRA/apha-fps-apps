using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class AccountCategoryViewModel
    {
        [Display(Name = "AccountShortName")]
        [Required(ErrorMessage = "Account Short Name is required")]
        [StringLength(50, ErrorMessage = "Account Short Name cannot exceed 50 characters")]
        [GridColumn(Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string AccShortName { get; set; } = null!;

        [Display(Name = "AccountDescription")]
        [StringLength(50, ErrorMessage = "Account Description cannot exceed 50 characters")]
        [GridColumn(Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AccountDescription { get; set; }

        [Display(Name = "ConstituentAccountCodes")]
        [StringLength(100, ErrorMessage = "Constituent Account Codes cannot exceed 100 characters")]
        [GridColumn(Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ConstituentAccountCodes { get; set; }

        [Display(Name = "AccountType")]
        [Required(ErrorMessage = "Account Type is required")]
        [StringLength(10, ErrorMessage = "Account Type cannot exceed 10 characters")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string AccountType { get; set; } = null!;

        [Display(Name = "For Project Specific Costs")]
        [GridColumn(Width = 150, Type = GridColumnType.Checkbox)]
        public int? ProjectSpecific { get; set; }

        [Display(Name = "For Resource Centres")]
        [GridColumn(Width = 150, Type = GridColumnType.Checkbox)]
        public int? RcSpecific { get; set; }

      //  public int? FpsYear { get; set; }
    }
}
