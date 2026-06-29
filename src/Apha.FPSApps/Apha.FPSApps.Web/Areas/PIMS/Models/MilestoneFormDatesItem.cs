using Apha.FPSApps.Web.Models.Components.DataGrid;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class MilestoneFormDatesItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string ParentProject { get; set; } = null!;

        [Required(ErrorMessage = "Financial Year is required")]
        [Range(1900, 2100, ErrorMessage = "Enter a valid year (1900–2100)")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must be a 4-digit number")]
        [Display(Name = "Financial Year")]
        [GridColumn(Order = 1, Width = 110, Type = GridColumnType.Text, IsFilterable = false)]
        public short? Year { get; set; }

        [Display(Name = "Jun")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.Date)]
        public DateTime? Jun { get; set; }

        [Display(Name = "Sep")]
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.Date)]
        public DateTime? Sep { get; set; }

        [Display(Name = "Dec")]
        [GridColumn(Order = 4, Width = 90, Type = GridColumnType.Date)]
        public DateTime? Dec { get; set; }

        [Display(Name = "Jan")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Date)]
        public DateTime? Jan { get; set; }

        [Display(Name = "Feb")]
        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.Date)]
        public DateTime? Feb { get; set; }

        [Display(Name = "Mar")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.Date)]
        public DateTime? Mar { get; set; }
    }
}
