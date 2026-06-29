
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class InvoiceViewModel
    {
        public string? FilterProject { get; set; }
      
        public string? FilterContract { get; set; }
        public int? FilterYear { get; set; }
        public string? FilterProgram { get; set; }

        // ── Filter dropdown lists ────────────────────────────────────────────────
        public List<SelectListItem> ProjectList { get; set; } = [];
        public List<SelectListItem> ContractList { get; set; } = [];

        public List<SelectListItem> YearList { get; set; } = [];
        public List<SelectListItem> ProgramList { get; set; } = [];

        // ── DataGrid ─────────────────────────────────────────────────────────────

        public DataGridConfig<InvoiceItem> InvoicesGrid { get; set; } = new();

        // ── Totals footer ────────────────────────────────────────────────────────

        public RadTrackInvoiceTotalsDto? InvoiceTotals { get; set; }
    }
}
