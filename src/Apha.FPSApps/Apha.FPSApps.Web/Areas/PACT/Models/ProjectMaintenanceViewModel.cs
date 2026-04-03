using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectListViewModel
    {
        public DataGridConfig<PactProjectViewModel> ProjectGrid { get; set; } = new DataGridConfig<PactProjectViewModel>();
        public DataGridConfig<ProjectJobCodeViewModel> ProjectJobCodeGrid { get; set; } = new DataGridConfig<ProjectJobCodeViewModel>();
        public int? ViewBy { get; set; } = 1;
    }

    public class ProjectMaintenanceViewModel
    {
        public PactProjectViewModel Project { get; set; } = new PactProjectViewModel();
        public DataGridConfig<JobCodeViewModel> JobCodeGrid { get; set; } = new DataGridConfig<JobCodeViewModel>();
        public DataGridConfig<TimeCodeViewModel> TimeCodeGrid { get; set; } = new DataGridConfig<TimeCodeViewModel>();
        public List<SelectListItem> Statuses { get; set; } = [];
        public List<SelectListItem> Diseases { get; set; } = [];
        public List<SelectListItem> Customers { get; set; } = [];
        public List<SelectListItem> Contracts { get; set; } = [];
        public List<SelectListItem> Programs { get; set; } = [];
        public List<SelectListItem> WorkGroups { get; set; } = [];
        public List<SelectListItem> Managers { get; set; } = [];
    }
}
