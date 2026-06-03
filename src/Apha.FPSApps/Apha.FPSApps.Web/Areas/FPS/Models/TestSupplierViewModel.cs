using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestSupplierViewModel
    {
        public string SelectedTestCode { get; set; } = string.Empty;
        public bool ShowRejected { get; set; } = false;
        public decimal TotalNoTests { get; set; }
        public decimal TotalTestCosts { get; set; }
        public DataGridConfig<TestSupplierItem> TestSupplierGrid { get; set; } = new();
        public List<SelectListItem> TestCodeOptions { get; set; } = new();
    }
}
