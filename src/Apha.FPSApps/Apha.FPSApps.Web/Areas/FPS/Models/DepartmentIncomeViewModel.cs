using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // Read-only report form — no CRUD DataGridConfigs; query results shown in modal on Run query click
    public class DepartmentIncomeViewModel
    {
        public string? SelectedProject { get; set; }

        // The visible control is periodFromDropdown (period-table-dropdown custom control)
        public int? SelectedMonthFrom { get; set; }

        // The visible control is periodToDropdown (period-table-dropdown custom control)
        public int? SelectedMonthTo { get; set; }

        // Populated from IProjectService.GetAllProjectsAsync() — uses ParentProject as both Value and Text
        public List<SelectListItem> ProjectList { get; set; } = new();

        // Populated from IDepartmentIncomeService.GetPeriodsAsync()
        // AccntsPeriod = accounting period (1–12), MonthName = display name, MonthNumber = calendar month
        public List<PeriodItem> PeriodList { get; set; } = new();

        // Populated from tblkpMonth (12 fiscal months, April=1…March=12)
        // Used by Period From / Period To dropdowns — matches Access combo box query:
        //   SELECT DISTINCTROW MonthNumber, MonthName FROM tblkpMonth ORDER BY MonthNumber
        public List<MonthItem> MonthList { get; set; } = new();

        // JS DataGridComponent columns: periodName (240px), finalSummariesRun (checkbox, 180px), periodLocke (checkbox, 150px)
        // AllowAdd=false, AllowEdit=false, AllowDelete=false — showAddButton: false in JS
        // Explicitly built in controller Index() — never left as new()
        public DataGridConfig<DepartmentIncomeSnapshotItem> SnapshotGrid { get; set; } = new();

        // Set when the FPS API is unreachable or returns an error during page load
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Grid row model for the Snapshot-tab DataGrid (JS getPeriodColumns() — 3 columns).
    /// Matches the snapshotData structure from fps_department_income.js:
    ///   periodName (240px, ReadOnly), finalSummariesRun (180px, Checkbox), periodLocke (150px, Checkbox)
    /// The snapshot data also carries projectCode and month (hidden, used for filtering in JS).
    /// </summary>
    public class DepartmentIncomeSnapshotItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? ProjectCode { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Month { get; set; }

        [Display(Name = "Period Name")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? PeriodName { get; set; }

        // render: renderBooleanCheckbox — GridColumnType.Checkbox
        [Display(Name = "Final Summaries Run")]
        [GridColumn(Width = 180, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool FinalSummariesRun { get; set; }

        // render: renderBooleanCheckbox — GridColumnType.Checkbox
        [Display(Name = "Period Locked")]
        [GridColumn(Width = 150, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool PeriodLocked { get; set; }
    }

    /// <summary>DTO posted from the Edit Snapshot Period modal to UpdateSnapshotPeriod action.</summary>
    public class DepartmentIncomeSnapshotUpdateDto
    {
        public string PeriodName { get; set; } = string.Empty;
        public bool PeriodLocked { get; set; }
    }

    /// <summary>
    /// Represents a single row from tblkpMonth (12 fiscal months).
    /// MonthNumber 1=April … 12=March, matching Access Period From/To combo boxes.
    /// </summary>
    public class MonthItem
    {
        public int MonthNumber { get; set; }
        public string MonthName { get; set; } = null!;
    }
}
