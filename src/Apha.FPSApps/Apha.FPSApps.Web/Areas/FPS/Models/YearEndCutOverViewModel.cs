using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class YearEndCutOverViewModel
    {
        public int PlannedYear { get; set; }
        public bool CanInitiate { get; set; }
        public bool CanApprove { get; set; }
        public Guid? CurrentJobExecutionId { get; set; }
        public required DataGridConfig<YearEndHistoryItem> HistoryGrid { get; set; }
    }
}
