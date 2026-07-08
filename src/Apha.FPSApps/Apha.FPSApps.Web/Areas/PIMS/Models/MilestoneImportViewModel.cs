using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class MilestoneImportViewModel
    {
        public string Parentproject { get; set; } = string.Empty;
        public bool Overwrite { get; set; }
        public List<SelectListItem> ProjectOptions { get; set; } = [];
        public List<SelectListItem> MilestoneTypeOptions { get; set; } = [];
        public DataGridConfig<StagingMilestoneItem> ImportGrid { get; set; } = new();
        public char TypeLookUp { get; set; }
        public string ImportType { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }
}