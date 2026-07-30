using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class SnapShotAnimalDataViewModel
    {
        public DataGridConfig<AnimalSnapshotItem> SnapShotAnimalDataGrid { get; set; } = new DataGridConfig<AnimalSnapshotItem>();
    }
}
