namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Request model for copying invoices from source month to target month.
    /// Supports both bulk copy (all invoices) and selective copy (specific invoice records).
    /// </summary>
    public class CopyInvoicesRequest
    {
        /// <summary>
        /// The month number to copy invoices from (1-12).
        /// </summary>
        public int SourceMonth { get; set; }

        /// <summary>
        /// The month number to copy invoices to (1-12).
        /// </summary>
        public int TargetMonth { get; set; }

        /// <summary>
        /// Optional list of specific invoice records to copy.
        /// If null or empty, all invoices from the source month will be copied.
        /// </summary>
        public List<AutomaticInvoiceItem>? InvoiceRecords { get; set; }
    }
}
