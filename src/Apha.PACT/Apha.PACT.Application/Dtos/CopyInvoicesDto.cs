namespace Apha.PACT.Application.Dtos
{
    public class CopyInvoicesDto
    {
        public int SourceMonth { get; set; }
        public int TargetMonth { get; set; }
        public List<int>? InvoiceIds { get; set; }

        /// <summary>
        /// Optional collection of invoice data for selective copy.
        /// If provided, these invoices will be copied instead of fetching from the database.
        /// </summary>
        public List<ProjectInvoiceDto>? InvoiceRecords { get; set; }
    }
}
