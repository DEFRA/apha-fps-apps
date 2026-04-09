using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class ProjectViewModel
    {
        public DataGridConfig<ProjectItemViewModel> ProjectGrid { get; set; } = new DataGridConfig<ProjectItemViewModel>();
        
        [Display(Name = "Select Year")]
        public int SelectedYear { get; set; } = 2025;
        public List<SelectListItem> YearOptions { get; set; }
        [Display(Name = "Search by Costbook Code")]
        //public string SearchTerm { get; set; }       
        public required string SearchTerm { get; set; } = string.Empty;
        public List<ProjectItemViewModel> Projects { get; set; }
        [Display(Name = "Records Per Page")]
        public int RecordsPerPage { get; set; } = 5;
        public List<SelectListItem> RecordsPerPageOptions { get; set; }
        [Display(Name = "Current Page")]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public ProjectViewModel()
        {
            Projects = new List<ProjectItemViewModel>();
            YearOptions = new List<SelectListItem>();
            RecordsPerPageOptions = new List<SelectListItem>();
            SelectedYear = 2025;
            RecordsPerPage = 5;
            CurrentPage = 1;
            TotalPages = 1;
            TotalRecords = 0;
        }
    }
}
