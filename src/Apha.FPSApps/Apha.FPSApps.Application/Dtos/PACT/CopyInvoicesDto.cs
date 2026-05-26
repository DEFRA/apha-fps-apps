namespace Apha.FPSApps.Application.Dtos.PACT
{
    /// <summary>
    /// DTO for copying invoices from one month to another.
    /// Supports both bulk copy (all invoices) and selective copy (specific invoice records).
    /// </summary>
    public class CopyInvoicesDto
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
        /// If null or empty, all invoices from the source month will be copied (bulk copy).
        /// If provided, only these specific invoices will be copied (selective copy).
        /// </summary>
        public List<ProjectInvoiceDto>? InvoiceRecords { get; set; }
    }
}
