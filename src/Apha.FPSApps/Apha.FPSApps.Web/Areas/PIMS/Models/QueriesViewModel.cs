using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class QueriesViewModel
    {
        public DataGridConfig<QueryResultItem> QueryResultsGrid { get; set; } = new();
    }
}
