using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupPeopleViewModel
    {
        public string? SelectedWorkGroup { get; set; }

        public DataGridConfig<WorkGroupPeopleItem> PeopleGrid { get; set; } = new();

        public List<WorkGroup> WorkGroupOptions { get; set; } = new();

        public List<WorkGroupPerson> PersonOptions { get; set; } = new();
    }
}
