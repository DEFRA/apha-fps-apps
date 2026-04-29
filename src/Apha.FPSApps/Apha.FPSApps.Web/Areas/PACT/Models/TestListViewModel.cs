using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestListViewModel
    {
        public DataGridConfig<TestOrProductViewModel> TestGrid { get; set; } = new();
    }
}
