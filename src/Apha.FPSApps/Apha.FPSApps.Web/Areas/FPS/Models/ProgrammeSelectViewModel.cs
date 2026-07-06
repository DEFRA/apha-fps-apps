using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for Programme Manager - a read-only navigation interface
    /// that lists projects filtered by selected programme
    /// </summary>
    public class ProgrammeSelectViewModel
    {
        /// <summary>
        /// Currently selected programme number (filter value)
        /// </summary>
        public string SelectedProgramNo { get; set; } = string.Empty;

        /// <summary>
        /// List of all programmes for the dropdown
        /// </summary>
        public List<SelectListItem> ProgrammeList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// List of projects within the selected programme
        /// </summary>
        public List<ProgrammeSelectProjectItem> Projects { get; set; } = new List<ProgrammeSelectProjectItem>();

        /// <summary>
        /// Current project text filter value.
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// DataGrid configuration for the projects table
        /// </summary>
        public DataGridConfig<ProgrammeSelectProjectItem>? ProjectsGrid { get; set; }
    }

    /// <summary>
    /// Simplified project item for the Programme Manager list
    /// </summary>
    public class ProgrammeSelectProjectItem
    {
        /// <summary>
        /// Programme code (e.g. "Bact", "ADMIN")
        /// </summary>
        [Display(Name = "Programme")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string Program { get; set; } = string.Empty;

        /// <summary>
        /// Project code (e.g. "FZ2000", "APHAB000000")
        /// </summary>
        [Display(Name = "Project Code")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string ParentProject { get; set; } = string.Empty;
    }
}
