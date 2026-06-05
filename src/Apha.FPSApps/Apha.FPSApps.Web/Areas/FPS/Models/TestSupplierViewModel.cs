using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestSupplierViewModel
    {
        public string SelectedTestCode { get; set; } = string.Empty;
        public bool ShowRejected { get; set; }
        public List<SelectListItem> TestCodeList { get; set; } = new List<SelectListItem>();
        public DataGridConfig<TestSupplierItem> TestSupplierGrid { get; set; } = new DataGridConfig<TestSupplierItem>();
    }
}
