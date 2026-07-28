using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BBQueryViewModel
    {
        public DataGridConfig<BBQueryCrosstabRow> Grid { get; set; } = new DataGridConfig<BBQueryCrosstabRow>();

        public List<SelectListItem> ProfitCentreOptions { get; set; } = new();

        public string? SelectedProfitCentre { get; set; }

        public int FpsYear { get; set; }
    }
}
