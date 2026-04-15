using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectListViewModel
    {     
    
        public DataGridConfig<ProjectListItem> ProjectGrid { get; set; } = new();
        public int FilterOption { get; set; } = 2;

    }
}
