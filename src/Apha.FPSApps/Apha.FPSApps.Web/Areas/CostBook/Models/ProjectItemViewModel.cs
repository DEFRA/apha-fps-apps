using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class ProjectItemViewModel
    {
        [GridColumn(Width = 271, Type = GridColumnType.Text, IsFilterable = true)]
        [Display(Name = "Project")]
        public required string ProjectId { get; set; }

        [GridColumn(Width = 271, Type = GridColumnType.Text)]
        public required string Projecttitle { get; set; }

        [GridColumn(Width = 271, Type = GridColumnType.Text)]
        public required string ContractNumber { get; set; }
       
        [GridColumn(Width = 271, Type = GridColumnType.Date)]
        public DateOnly? Startdate { get; set; }

        [GridColumn(IsVisible =false)]
        public bool Isdefraproject { get; set; }
    }
}
