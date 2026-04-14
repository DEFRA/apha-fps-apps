using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestPurchaseRequirementViewModel
    {
        public string ParentProject { get; set; } = string.Empty;
        public DataGridConfig<TestPurchaseRequirementItem> TestPurchaseReqGrid { get; set; } = new();
    }
}
