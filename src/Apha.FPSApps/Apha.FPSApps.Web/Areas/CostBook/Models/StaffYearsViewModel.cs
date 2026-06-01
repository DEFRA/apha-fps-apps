using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class StaffYearsViewModel
    {
        public string ProjectId { get; set; } = null!;
        public ProjectHeaderDto ProjectHeaderDto { get; set; } = new();
        public DataGridConfig<StaffYearsPivotRow> Grid { get; set; } = new DataGridConfig<StaffYearsPivotRow>();
    }
}
