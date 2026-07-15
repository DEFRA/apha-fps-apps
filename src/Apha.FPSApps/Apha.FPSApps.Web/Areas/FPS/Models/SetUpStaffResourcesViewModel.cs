using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class SetUpStaffResourcesViewModel
    {
        // TRANSFORMENGINE: Resource Centre dropdown — populated from IProfitCentreService.GetAllProfitCentresAsync()
        public List<SelectListItem> ResourceCentres { get; set; } = new List<SelectListItem>();

        /// <summary>Currently selected Resource Centre (profit centre ID).</summary>
        public string SelectedResourceCentre { get; set; } = string.Empty;

        // TRANSFORMENGINE: Grade/WorkGroup list — populated from IWorkGroupGradeService.GetWorkGroupGradeAsync(profitCentre)
        //   Each entry is a WgGrade code shown in the left-panel listbox (ssrGradeList)
        public List<string> GradeList { get; set; } = new List<string>();

        /// <summary>Maps each WgGrade to its GradeCode for summary box binding (ssrSummaryGrade).</summary>
        public Dictionary<string, string> GradeCodeMap { get; set; } = new Dictionary<string, string>();

        /// <summary>Currently selected WorkGroup Grade code.</summary>
        public string SelectedGrade { get; set; } = string.Empty;

        /// <summary>Selected WorkGroup display text (ssrSelectedWorkgroup input).</summary>
        public string SelectedWorkgroup { get; set; } = string.Empty;

        // TRANSFORMENGINE: Staff DataGrid — populated from IWorkGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync()
        public DataGridConfig<SetUpStaffResourcesItem> StaffGrid { get; set; } = new DataGridConfig<SetUpStaffResourcesItem>();

        // TRANSFORMENGINE: Summary panel fields — computed server-side from staff grid data
        /// <summary>Grade code shown in summary box (ssrSummaryGrade).</summary>
        public string SummaryGrade { get; set; } = string.Empty;

        /// <summary>Total HrsAvail at staff level for selected grade (ssrWorkHrs).</summary>
        public double SummaryWorkHrs { get; set; }

        /// <summary>Name of currently selected person (ssrPersonSelected).</summary>
        public string SelectedPersonName { get; set; } = string.Empty;
    }
}
