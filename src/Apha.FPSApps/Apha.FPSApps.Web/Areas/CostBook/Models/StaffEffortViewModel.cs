using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class StaffEffortViewModel
    {
        public string ProjectId { get; set; } = null!;
        public ProjectHeaderDto ProjectHeaderDto { get; set; } = new();
        public DataGridConfig<StaffEffortPivotRow> Grid { get; set; } = new DataGridConfig<StaffEffortPivotRow>();
    }
}
