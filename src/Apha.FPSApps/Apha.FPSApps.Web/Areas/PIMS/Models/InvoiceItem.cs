// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — InvoiceItem.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: grid row + modal model for the Invoice data grid.
 *   - Properties derived from pimsinvoices.js renderInvTable() column rendering
 *     and frmpimsinvoices.html <thead> column headers.
 *   - Column order follows the HTML prototype table left-to-right:
 *     Project, Contract, PlannedAmount, DueAmount, DueDate, ActualAmount,
 *     DateJobsheetRaised, InvoiceRef, InvoicePaid, DateInvoiced.
 *   - InvoiceCounter: PK — hidden (not rendered as a visible JS column, used only
 *     as KeyProperty for edit/delete row identification).
 *   - InvoicePaid: short (NOT bool) — matches RadTrackInvoiceDto.InvoicePaid (smallint)
 *     and uses GridColumnType.Checkbox; rendered as tick/empty in renderInvTable().
 *   - Currency columns (PlannedAmount, DueAmount, ActualAmount) use GbpValue type
 *     matching the invFmt() helper in pimsinvoices.js.
 *   - AutoMapper convention mapping: InvoiceItem <-> RadTrackInvoiceDto
 *     (registered in PimsViewModelMapper.cs Phase 10). All property names match
 *     RadTrackInvoiceDto exactly so no .ForMember() overrides are required.
 *
 * PRESERVED:
 *   - All 10 visible columns from frmpimsinvoices.html <thead> preserved verbatim.
 *   - Field names match RadTrackInvoiceDto to enable convention-based AutoMapper mapping.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: InvoicePaid is short (smallint 0/1) — if backend DTO changes
 *     to bool, update type here and adjust the AutoMapper profile accordingly.
 *   - TRANSFORMENGINE TODO: Project field in modal is a <select> in the prototype. The
 *     controller populates ProjectList for the dropdown; verify the _AddEditInvoice partial
 *     renders Project as <select asp-for="Project" asp-items="..."> rather than a text input.
 *   - TRANSFORMENGINE TODO: Date fields (DueDate, DateJobsheetRaised, DateInvoiced) are
 *     stored as DateTime? on the DTO. The HTML prototype uses DD/MM/YYYY text inputs.
 *     Verify date parsing in the controller Create/Edit POST actions.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class InvoiceItem
    {
        // TRANSFORMENGINE: PK — hidden column; used as KeyProperty for Edit/Delete row identification.
        // Not a visible column in pimsinvoices.js renderInvTable() (id field is action-only).
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int InvoiceCounter { get; set; }

        // TRANSFORMENGINE: Column 1 — matches JS field 'project', header "Project".
        // Editable in modal as <select>; filterable in grid.
        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Project { get; set; }

        // TRANSFORMENGINE: Column 2 — matches JS field 'contract', header "Contract".
        [Display(Name = "Contract")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Contract { get; set; }

        // TRANSFORMENGINE: Column 3 — matches JS field 'plannedAmt', header "Planned Amount".
        // invFmt() renders as £ currency; maps to GbpValue column type.
        [Display(Name = "Planned Amount")]
        [GridColumn(Order = 3, Width = 130, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public double? PlannedAmount { get; set; }

        // TRANSFORMENGINE: Column 4 — matches JS field 'amountDue', header "Amount Due".
        [Display(Name = "Amount Due")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public double? DueAmount { get; set; }

        // TRANSFORMENGINE: Column 5 — matches JS field 'dateDue', header "Date Due".
        [Display(Name = "Date Due")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DueDate { get; set; }

        // TRANSFORMENGINE: Column 6 — matches JS field 'amtInvoiced', header "Amount Invoiced".
        [Display(Name = "Amount Invoiced")]
        [GridColumn(Order = 6, Width = 130, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public double? ActualAmount { get; set; }

        // TRANSFORMENGINE: Column 7 — matches JS field 'dateJSRaised', header "Date JS Raised".
        [Display(Name = "Date JS Raised")]
        [GridColumn(Order = 7, Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateJobsheetRaised { get; set; }

        // TRANSFORMENGINE: Column 8 — matches JS field 'invoiceRef', header "Invoice Ref".
        [Display(Name = "Invoice Ref")]
        [GridColumn(Order = 8, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string? InvoiceRef { get; set; }

        // TRANSFORMENGINE: Column 9 — matches JS field 'paid', header "Paid?".
        // renderInvTable() renders ✔ when paid=true, empty otherwise → GridColumnType.Checkbox.
        // Type is short (smallint 0/1) matching RadTrackInvoiceDto.InvoicePaid.
        [Display(Name = "Paid?")]
        [GridColumn(Order = 9, Width = 70, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public short InvoicePaid { get; set; }

        // TRANSFORMENGINE: Column 10 — matches JS field 'dateInvoiced', header "Date Invoiced".
        [Display(Name = "Date Invoiced")]
        [GridColumn(Order = 10, Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateInvoiced { get; set; }
    }
}
