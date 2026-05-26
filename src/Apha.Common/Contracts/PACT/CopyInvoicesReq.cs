namespace Apha.Common.Contracts.PACT
{
    public class CopyInvoicesReq
    {
        public int SourceMonth { get; set; }
        public int TargetMonth { get; set; }
        public List<int>? InvoiceIds { get; set; }

        /// <summary>
        /// Optional collection of invoice data for selective copy.
        /// If provided, these invoices will be copied instead of fetching from the database.
        /// </summary>
        public List<ProjectInvoiceReq>? InvoiceRecords { get; set; }
    }
}
