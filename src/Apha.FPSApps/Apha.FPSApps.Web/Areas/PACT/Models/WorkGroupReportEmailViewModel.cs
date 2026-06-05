using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupReportEmailViewModel
    {
        // ── Filter bar ──────────────────────────────────────────────────────
        public string? SelectedProfitCentre { get; set; }
        public short? SelectedMonthNumber { get; set; }

        // ── Attachment flags (mirror Access TimeSheet / OutputSheet checks) ─
        public bool SendTimeSheet { get; set; }
        public bool SendOutputSheet { get; set; }

        // ── Time sheet layout flags ───────────────────────────────────────────
        public bool TimesheetLayoutFlat { get; set; }
        public bool TimesheetLayoutCrossTab { get; set; }

        // ── Dropdowns ────────────────────────────────────────────────────────
        public List<CalenderMonthDto> CalenderMonthItems { get; set; } = new();
        public List<SelectListItem> ProfitCentreOptions { get; set; } = new();

        // ── Work-group grid (uses shared DataGrid component) ─────────────────
        public DataGridConfig<WorkGroupEmailItem> WorkGroupGrid { get; set; } = new();
    }
}
