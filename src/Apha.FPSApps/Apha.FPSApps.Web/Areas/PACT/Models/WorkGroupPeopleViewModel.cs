using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupPeopleViewModel
    {
        public string? SelectedWorkGroup { get; set; }

        public DataGridConfig<WorkGroupPeopleItem> PeopleGrid { get; set; } = new();

        public List<WorkGroupDto> WorkGroupOptions { get; set; } = new();

        public List<WorkGroupPersonDto> PersonOptions { get; set; } = new();
    }
}
