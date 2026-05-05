using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Page ViewModel for the Resource Set-Up page.
    /// Holds three linked DataGrids: RC Grades, WG Grades, and WG Staff.
    /// The ProfitCentre dropdown drives all three grids via AJAX cascade.
    /// </summary>
    public class ResourceSetUpViewModel
    {
        /// <summary>The selected profit centre code.</summary>
        public string? ProfitCentre { get; set; }

        /// <summary>Profit centre dropdown list.</summary>
        public List<SelectListItem> ProfitCentreList { get; set; } = new();

        /// <summary>RC Grades Available grid (fsubpCGrade — read-only).</summary>
        public DataGridConfig<RcGradeItem> RcGradeGrid { get; set; } = new DataGridConfig<RcGradeItem>();

        /// <summary>WG Grades Available grid (fsubWGGrade — delete only).</summary>
        public DataGridConfig<WgGradeItem> WgGradeGrid { get; set; } = new DataGridConfig<WgGradeItem>();

        /// <summary>Staff of WG Grade grid (fsubWGStaff — edit + delete).</summary>
        public DataGridConfig<WgStaffItem> WgStaffGrid { get; set; } = new DataGridConfig<WgStaffItem>();

        /// <summary>
        /// Name of the selected person for the "Person Selected" aside panel.
        /// </summary>
        public string? SelectedPersonName { get; set; }

        /// <summary>
        /// PACTid of the selected person (hidden — used for Plan Person onto ZT Codes navigation).
        /// </summary>
        public string? SelectedPactId { get; set; }

        // Footer totals from fsubWGStaff
        public double WgHrsPaid { get; set; }
        public double WgLeave { get; set; }
        public double WgSickSpecial { get; set; }
        public double WgHrsAvail { get; set; }
    }
}
