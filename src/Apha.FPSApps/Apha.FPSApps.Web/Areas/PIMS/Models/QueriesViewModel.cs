using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class QueriesViewModel
    {
        public DataGridConfig<QueryResultItem> QueryResultsGrid { get; set; } = new();
        public List<SelectListItem> ContractOptions { get; set; } = [];
        public List<SelectListItem> YearOptions { get; set; } = [];
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
    }
}
